using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace Cogito.Text.Json
{

    /// <summary>
    /// Provides the capability of generating LINQ Expression trees to test whether a <see cref="JsonElement"/> instance is
    /// equal with another.
    /// </summary>
    public class JsonElementEqualityExpressionBuilder
    {

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


        readonly JsonElementEqualityExpressionBuilderSettings settings;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="settings"></param>
        public JsonElementEqualityExpressionBuilder(JsonElementEqualityExpressionBuilderSettings settings = null)
        {
            this.settings = settings ?? new JsonElementEqualityExpressionBuilderSettings();
        }

        /// <summary>
        /// Builds an expression tree that implements validation of JSON.
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public Expression<JsonElementPredicate> Build(JsonDocument template)
        {
            if (template is null)
                throw new ArgumentNullException(nameof(template));

            return Build(template.RootElement);
        }

        /// <summary>
        /// Builds an expression tree that implements validation of JSON.
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public Expression<JsonElementPredicate> Build(JsonElement template)
        {
            var t = Expression.Parameter(typeof(JsonElement), "target");
            var e = Build(template, t);
            return Expression.Lambda<JsonElementPredicate>(e, t);
        }

        /// <summary>
        /// Builds an expression tree which evaluates whether the <see cref="JsonElement"/> refered to by <paramref name="target"/>
        /// is the same as the <paramref name="template"/>.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public Expression Build(JsonElement template, Expression target)
        {
            var v = Expression.Variable(target.Type);

            return Expression.Block(
                typeof(bool),
                new[] { v },
                Expression.Assign(v, target),
                Eval(template, v));
        }

        Expression Eval(JsonElement template, ParameterExpression target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            switch (template.ValueKind)
            {
                case JsonValueKind.Array:
                    return BuildArray(template, target);
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return BuildBoolean(template, target);
                case JsonValueKind.Number:
                    return BuildNumber(template, target);
                case JsonValueKind.Null:
                    return BuildNull(template, target);
                case JsonValueKind.Object:
                    return BuildObject(template, target);
                case JsonValueKind.String:
                    return BuildString(template, target);
                default:
                    throw new InvalidOperationException("Unsupported token type in template.");
            }
        }

        Expression BuildArray(JsonElement template, Expression target)
        {
            return Expression.AndAlso(
                Expression.Equal(
                    Expression.Property(target, nameof(JsonElement.ValueKind)),
                    Expression.Constant(JsonValueKind.Array)),
                AllOf(BuildArrayEval(template, target)));
        }

        /// <summary>
        /// Iterates an expression that compares each position of each array.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        IEnumerable<Expression> BuildArrayEval(JsonElement template, Expression target)
        {
            var l = new List<Expression>(template.GetArrayLength() + 1);

            l.Add(Expression.Equal(
                Expression.Call(target, nameof(JsonElement.GetArrayLength), new Type[] { }),
                Expression.Constant(template.GetArrayLength())));

            for (var i = 0; i < template.GetArrayLength(); i++)
            {
                var v = template[i];
                l.Add(Build(v, Expression.Property(target, "Item", Expression.Constant(i))));
            }

            return l;
        }

        Expression BuildBoolean(JsonElement template, Expression target)
        {
            return Expression.Equal(
                Expression.Property(target, nameof(JsonElement.ValueKind)),
                Expression.Constant(template.ValueKind));
        }

        Expression BuildFloat(double template, Expression target)
        {
            var v = Expression.Variable(typeof(double?));

            return Expression.AndAlso(
                Expression.Equal(
                    Expression.Property(target, nameof(JsonElement.ValueKind)),
                    Expression.Constant(JsonValueKind.Number)),
                Expression.Block(
                    new[] { v },
                    Expression.Assign(
                        v,
                        Expression.Call(
                            GetDoubleOrNullMethodInfo,
                            target)),
                    Expression.AndAlso(
                        Expression.Property(v, nameof(Nullable<JsonElement>.HasValue)),
                        Expression.Equal(
                            Expression.Property(v, nameof(Nullable<JsonElement>.Value)),
                            Expression.Constant(template)))));
        }

        static readonly MethodInfo GetDoubleOrNullMethodInfo = typeof(JsonElementEqualityExpressionBuilder).GetMethod(nameof(GetDoubleOrNull), BindingFlags.Static | BindingFlags.NonPublic);

        static double? GetDoubleOrNull(JsonElement element)
        {
            return element.TryGetDouble(out var d) ? d : (double?)null;
        }

        Expression BuildInteger(long template, Expression target)
        {
            var v = Expression.Variable(typeof(long?));

            return Expression.AndAlso(
                Expression.Equal(
                    Expression.Property(target, nameof(JsonElement.ValueKind)),
                    Expression.Constant(JsonValueKind.Number)),
                Expression.Block(
                    new[] { v },
                    Expression.Assign(
                        v,
                        Expression.Call(
                            GetInt64OrNullMethodInfo,
                            target)),
                    Expression.AndAlso(
                        Expression.Property(v, nameof(Nullable<JsonElement>.HasValue)),
                        Expression.Equal(
                            Expression.Property(v, nameof(Nullable<JsonElement>.Value)),
                            Expression.Constant(template)))));
        }

        static readonly MethodInfo GetInt64OrNullMethodInfo = typeof(JsonElementEqualityExpressionBuilder).GetMethod(nameof(GetInt64OrNull), BindingFlags.Static | BindingFlags.NonPublic);

        static long? GetInt64OrNull(JsonElement element)
        {
            return element.TryGetInt64(out var l) ? l : (long?)null;
        }

        Expression BuildNumber(JsonElement template, Expression target)
        {
            if (template.TryGetInt64(out var l))
                return BuildInteger(l, target);

            if (template.TryGetDouble(out var d))
                return BuildFloat(d, target);

            throw new InvalidOperationException();
        }

        Expression BuildNull(JsonElement template, Expression target)
        {
            return Expression.Equal(
                Expression.Property(target, nameof(JsonElement.ValueKind)),
                Expression.Constant(JsonValueKind.Null));
        }

        Expression BuildObject(JsonElement template, Expression target)
        {
            return Expression.AndAlso(
                Expression.Equal(
                    Expression.Property(target, nameof(JsonElement.ValueKind)),
                    Expression.Constant(JsonValueKind.Object)),
                AllOf(BuildObjectEval(template, target)));
        }

        /// <summary>
        /// Iterates an expression that compares each position of each array.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        IEnumerable<Expression> BuildObjectEval(JsonElement template, Expression target)
        {
            yield return
                Expression.Equal(
                    Expression.Call(
                        typeof(Enumerable),
                        nameof(Enumerable.Count),
                        new[] { typeof(JsonProperty) },
                        Expression.Convert(
                            Expression.Call(
                                target,
                                nameof(JsonElement.EnumerateObject),
                                EmptyTypes),
                            typeof(IEnumerable<JsonProperty>))),
                    Expression.Constant(template.EnumerateObject().Count()));

            foreach (var property in template.EnumerateObject())
            {
                var p = property.Value;
                var v = Expression.Variable(typeof(JsonElement?));

                yield return Expression.Block(
                    new[] { v },
                    Expression.Assign(
                        v,
                        Expression.Call(
                            GetPropertyOrNullMethodInfo,
                            target,
                            Expression.Constant(property.Name))),
                    Expression.AndAlso(
                        Expression.Property(v, nameof(Nullable<JsonElement>.HasValue)),
                        Build(p, Expression.Property(v, nameof(Nullable<JsonElement>.Value)))));
            }
        }

        static readonly MethodInfo GetPropertyOrNullMethodInfo = typeof(JsonElementEqualityExpressionBuilder).GetMethod(nameof(GetPropertyOrNull), BindingFlags.Static | BindingFlags.NonPublic);

        static JsonElement? GetPropertyOrNull(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var v) ? (JsonElement?)v : null;
        }

        Expression BuildString(JsonElement template, Expression target)
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
