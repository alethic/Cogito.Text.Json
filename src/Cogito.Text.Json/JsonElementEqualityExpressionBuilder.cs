using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.Json;

namespace Cogito.Text.Json
{

    /// <summary>
    /// Provides the capability of generating LINQ Expression trees to test whether a <see cref="JsonElement"/> instance is
    /// equal with another.
    /// </summary>
    public class JsonElementEqualityExpressionBuilder
    {

        public delegate bool JsonElementPredicateDelegate(ref JsonElement element);

        static readonly Expression True = Expression.Constant(true);
        static readonly Expression False = Expression.Constant(false);

        /// <summary>
        /// Returns an expression that returns <c>true</c> if all of the given expressions returns <c>true</c>.
        /// </summary>
        /// <param name="expressions"></param>
        /// <returns></returns>
        static Expression AllOf(IEnumerable<Expression> expressions)
        {
            Expression e = null;

            foreach (var i in expressions)
                e = e == null ? i : Expression.AndAlso(e, i);

            return e;
        }

        /// <summary>
        /// Returns an expression that returns <c>true</c> if all of the given expressions returns <c>true</c>.
        /// </summary>
        /// <param name="expressions"></param>
        /// <returns></returns>
        static Expression AllOf(params Expression[] expressions) =>
            AllOf((IEnumerable<Expression>)expressions);

        /// <summary>
        /// Builds an expression tree that implements validation of JSON.
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public Expression<JsonElementPredicateDelegate> Build(ref JsonElement template)
        {
            var t = Expression.Parameter(typeof(JsonElement), "target");
            var e = Build(ref template, t);
            return Expression.Lambda<JsonElementPredicateDelegate>(e, t);
        }

        /// <summary>
        /// Builds an expression tree which evaluates whether the <see cref="JToken"/> refered to by <paramref name="target"/>
        /// is the same as the <paramref name="template"/>.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public Expression Build(ref JsonElement template, Expression target)
        {
            var v = Expression.Variable(target.Type);

            return Expression.Block(
                typeof(bool),
                new[] { v },
                Expression.Assign(v, target),
                Eval(ref template, v));
        }

        Expression Eval(ref JsonElement template, ParameterExpression target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            switch (template.ValueKind)
            {
                case JsonValueKind.Array:
                    return BuildArray(ref template, target);
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return BuildBoolean(ref template, target);
                case JsonValueKind.Number:
                    return BuildNumber(ref template, target);
                case JsonValueKind.Null:
                    return BuildNull(ref template, target);
                case JsonValueKind.Object:
                    return BuildObject(ref template, target);
                case JsonValueKind.String:
                    return BuildString(ref template, target);
                default:
                    throw new InvalidOperationException("Unsupported token type in template.");
            }
        }

        Expression BuildArray(ref JsonElement template, Expression target)
        {
            return Expression.Condition(
                Expression.AndAlso(
                    Expression.TypeIs(target, typeof(JsonElement)),
                    Expression.Equal(
                        Expression.Property(target, nameof(JsonElement.ValueKind)),
                        Expression.Constant(JsonValueKind.Array))),
                AllOf(BuildArrayEval(ref template, Expression.Convert(target, typeof(JsonElement)))),
                False);
        }

        /// <summary>
        /// Iterates an expression that compares each position of each array.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        IEnumerable<Expression> BuildArrayEval(ref JsonElement template, Expression target)
        {
            var l = new List<Expression>(template.GetArrayLength() + 1);

            l.Add(Expression.Equal(
                Expression.Call(target, nameof(JsonElement.GetArrayLength), new Type[] { }),
                Expression.Constant(template.GetArrayLength())));

            for (var i = 0; i < template.GetArrayLength(); i++)
            {
                var v = template[i];
                l.Add(Build(ref v, Expression.Property(target, "Item", Expression.Constant(i))));
            }

            return l;
        }

        Expression BuildBoolean(ref JsonElement template, Expression target)
        {
            switch (template.ValueKind)
            {
                case JsonValueKind.True:
                    return BuildValue<bool>(JsonValueKind.True, ref template, target);
                case JsonValueKind.False:
                    return BuildValue<bool>(JsonValueKind.False, ref template, target);
                default:
                    throw new InvalidOperationException();
            }
        }

        Expression BuildFloat(double template, Expression target)
        {
            var t = Expression.Convert(target, typeof(JsonElement));

            return Expression.Switch(
                Expression.Property(target, nameof(JToken.Type)),
                False,
                Expression.SwitchCase(
                    Expression.Equal(
                        Expression.Convert(Expression.Property(t, nameof(JValue.Value)), typeof(double)),
                        Expression.Constant((double)template.Value)),
                    Expression.Constant(JTokenType.Float)),
                Expression.SwitchCase(
                    Expression.Equal(
                        Expression.Convert(Expression.Convert(Expression.Property(t, nameof(JValue.Value)), typeof(long)), typeof(double)),
                        Expression.Constant((double)template.Value)),
                    Expression.Constant(JTokenType.Integer)));
        }

        Expression BuildInteger(long template, Expression target)
        {
            var t = Expression.Convert(target, typeof(JsonElement));

            return Expression.Switch(
                Expression.Property(target, nameof(JToken.Type)),
                False,
                Expression.SwitchCase(
                    Expression.Equal(
                        Expression.Convert(Expression.Property(t, nameof(JValue.Value)), typeof(long)),
                        Expression.Constant((long)template.Value)),
                    Expression.Constant(JTokenType.Integer)),
                Expression.SwitchCase(
                    Expression.Equal(
                        Expression.Convert(Expression.Property(t, nameof(JValue.Value)), typeof(double)),
                        Expression.Constant((double)(long)template.Value)),
                    Expression.Constant(JTokenType.Float)));
        }

        Expression BuildNumber(ref JsonElement template, Expression target)
        {
            if (template.TryGetInt64(out var l))
                return BuildInteger(l, target);

            if (template.TryGetDouble(out var d))
                return BuildFloat(d, target);

            throw new InvalidOperationException();
        }

        Expression BuildNull(ref JsonElement template, Expression target)
        {
            return BuildValue<object>(JsonValueKind.Null, ref template, target);
        }

        Expression BuildObject(ref JsonElement template, Expression target)
        {
            return Expression.Condition(
                Expression.AndAlso(
                    Expression.TypeIs(target, typeof(JObject)),
                    Expression.Equal(
                        Expression.Property(target, nameof(JsonElement.ValueKind)),
                        Expression.Constant(JsonValueKind.Object))),
                AllOf(BuildObjectEval(template, Expression.Convert(target, typeof(JObject)))),
                False);
        }

        /// <summary>
        /// Iterates an expression that compares each position of each array.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        IEnumerable<Expression> BuildObjectEval(ref JsonElement template, Expression target)
        {
            var l = new List<Expression>();

            l.Add(Expression.Equal(
                Expression.Property(target, nameof(JObject.Count)),
                Expression.Constant(template.Count)));

            foreach (var p in template.Properties())
                l.Add(Expression.AndAlso(
                    Expression.IsTrue(
                        Expression.Call(
                            target,
                            nameof(JObject.ContainsKey),
                            new Type[0],
                            new[] { Expression.Constant(p.Name) })),
                    Build(
                        p.Value,
                        Expression.Call(
                            target,
                            nameof(JObject.GetValue),
                            new Type[0],
                            new[] { Expression.Constant(p.Name) }))));

            return l;
        }

        Expression BuildString(ref JsonElement template, Expression target)
        {
            return BuildValue<string>(JsonValueKind.String, ref template, target);
        }

        Expression BuildValue<T>(JsonValueKind type, ref JsonElement template, Expression target)
        {
            var t = Expression.Convert(target, typeof(JsonElement));
            var v = Expression.Convert(Expression.Property(t, nameof(JValue.Value)), typeof(T));

            return Expression.Condition(
                Expression.AndAlso(
                    Expression.TypeIs(target, typeof(JsonElement)),
                    Expression.Equal(
                        Expression.Property(target, nameof(JsonElement.ValueKind)),
                        Expression.Constant(type))),
                Expression.Equal(Expression.Constant((T)template.Value), v),
                False);
        }

    }

}
