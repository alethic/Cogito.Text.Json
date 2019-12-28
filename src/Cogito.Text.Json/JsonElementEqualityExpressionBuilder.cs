using System;
using System.Collections.Generic;
using System.Linq;
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
        static readonly Type[] EmptyTypes = new Type[0];

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
        public Expression<JsonElementPredicateDelegate> Build(JsonDocument template)
        {
            if (template is null)
                throw new ArgumentNullException(nameof(template));

            var root = template.RootElement;
            return Build(ref root);
        }

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
            return Expression.Equal(
                Expression.Property(target, nameof(JsonElement.ValueKind)),
                Expression.Constant(template.ValueKind));
        }

        Expression BuildFloat(double template, Expression target)
        {
            var d = Expression.Parameter(typeof(double).MakeByRefType(), "d");

            return Expression.AndAlso(
                Expression.Equal(
                    Expression.Property(target, nameof(JsonElement.ValueKind)),
                    Expression.Constant(JsonValueKind.Number)),
                Expression.AndAlso(
                    Expression.Call(target, typeof(JsonElement).GetMethod(nameof(JsonElement.TryGetDouble)), d),
                    Expression.Equal(
                        d,
                        Expression.Constant(template))));
        }

        Expression BuildInteger(long template, Expression target)
        {
            var l = Expression.Parameter(typeof(long).MakeByRefType(), "l");

            return Expression.AndAlso(
                Expression.Equal(
                    Expression.Property(target, nameof(JsonElement.ValueKind)),
                    Expression.Constant(JsonValueKind.Number)),
                Expression.AndAlso(
                    Expression.Call(target, typeof(JsonElement).GetMethod(nameof(JsonElement.TryGetInt64)), l),
                    Expression.Equal(
                        l,
                        Expression.Constant(template))));
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
            return Expression.Equal(
                Expression.Property(target, nameof(JsonElement.ValueKind)),
                Expression.Constant(JsonValueKind.Null));
        }

        Expression BuildObject(ref JsonElement template, Expression target)
        {
            return Expression.AndAlso(
                Expression.Equal(
                    Expression.Property(target, nameof(JsonElement.ValueKind)),
                    Expression.Constant(JsonValueKind.Object)),
                AllOf(BuildObjectEval(ref template, target)));
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

            // target's property count is equal to template's property count
            l.Add(
                Expression.Equal(
                    Expression.Call(
                        typeof(Enumerable),
                        nameof(Enumerable.Count),
                        new[] { typeof(JsonProperty) },
                        Expression.Call(
                            target,
                            nameof(JsonElement.EnumerateObject),
                            EmptyTypes)),
                    Expression.Constant(template.EnumerateObject().Count())));

            foreach (var p in template.EnumerateObject())
            {
                var t = p.Value;
                var v = Expression.Parameter(typeof(JsonElement).MakeByRefType(), "v");

                // invoke TryGetProperty(out var v) && Build(p, v)
                l.Add(
                    Expression.AndAlso(
                        Expression.Call(
                            target,
                            nameof(JsonElement.TryGetProperty),
                            EmptyTypes,
                            Expression.Constant(p.Name),
                            v),
                        Build(
                            ref t,
                            v)));
            }

            return l;
        }

        Expression BuildString(ref JsonElement template, Expression target)
        {
            return Expression.AndAlso(
                Expression.Equal(
                    Expression.Property(target, nameof(JsonElement.ValueKind)),
                    Expression.Constant(JsonValueKind.String)),
                Expression.Equal(
                    Expression.Call(target, nameof(JsonElement.GetString), EmptyTypes),
                    Expression.Constant(template.GetString())));
        }

    }

}
