using System.Linq.Expressions;

// I.
// https://stackoverflow.com/questions/44408583/linq-query-to-check-for-a-predicate-in-all-columns-in-a-table/

class Strange
{
    public int A { get; set; }
    public double B { get; set; }
    public string C { get; set; }
    public int D { get; set; }
    public double E { get; set; }
    public string F { get; set; }
    public int G { get; set; }
    public double H { get; set; }
    public string I { get; set; }

    public Strange(int a, double b, string c, int d, double e, string f, int g, double h, string i)
    {
        A = a; B = b; C = c; D = d; E = e; F = f; G = g; H = h; I = i;
    }
}

var stranges = new[]
{
    new Strange(1, 2.3, "Abc", 4, 5.6, "Def", 7, 8.9, "Ghi"),
    new Strange(7, 8.9, "Ghi", 1, 2.3, "Abc", 4, 5.6, "Def"),
    new Strange(4, 5.6, "Def", 7, 8.9, "Ghi", 1, 2.3, "Abc"),
};

Expression<Func<TObject, bool>> BuildMultiEquality<TObject, TValue>(TValue value)
{
    var parameter = Expression.Parameter(typeof(TObject));
    var valueExpression = Expression.Constant(value, typeof(TValue));
    Expression result = Expression.Constant(false, typeof(bool));

    var properties = typeof(TObject).GetProperties()
                                    .Where(p => p.PropertyType == typeof(TValue));

    foreach (var property in properties)
    {
        var propertyExpression = Expression.Property(parameter, property);
        var equal = Expression.Equal(propertyExpression, valueExpression);

        result = Expression.Or(result, equal);
    }

    return Expression.Lambda<Func<TObject, bool>>(result, parameter);
}

var filter1 = BuildMultiEquality<Strange, int>(5).Compile();
stranges.Where(filter1).Count()

var filter2 = BuildMultiEquality<Strange, int>(4).Compile();
stranges.Where(filter2).Count()

// II.

class DerivativeVisitor : ExpressionVisitor
{
}

LambdaExpression Derivative(Expression<Func<double, double>> f)
{
    var derivativeVisitor = new DerivativeVisitor();

    return Expression.Lambda(derivativeVisitor.Visit(f.Body), f.Parameters);
}

var a = 15.0;
Derivative(x => a * x * (x + 3)).ToString()

class DerivativeVisitor : ExpressionVisitor
{
    protected override Expression VisitConstant(ConstantExpression node)
    {
        return Expression.Constant(0.0);
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        return Expression.Constant(1.0);
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        return Expression.Constant(0.0);
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        switch (node.NodeType)
        {
            case ExpressionType.Add:
                return Expression.Add(Visit(node.Left), Visit(node.Right));

            case ExpressionType.Multiply:
                var addend = Expression.Multiply(node.Left, Visit(node.Right));
                var augend = Expression.Multiply(Visit(node.Left), node.Right);
                return Expression.Add(addend, augend);

            default:
                throw new InvalidOperationException("Binary operator is not applicable for derivative");
        }
    }
}

using System.Diagnostics;

class DerivativeVisitor : ExpressionVisitor
{
    protected override Expression VisitConstant(ConstantExpression node)
    {
        return Expression.Constant(0.0);
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        return Expression.Constant(1.0);
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        return Expression.Constant(0.0);
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        switch (node.NodeType)
        {
            case ExpressionType.Add:
                return Add(Visit(node.Left), Visit(node.Right));

            case ExpressionType.Multiply:
                var addend = Multiply(node.Left, Visit(node.Right));
                var augend = Multiply(Visit(node.Left), node.Right);
                return Add(addend, augend);

            default:
                throw new InvalidOperationException("Binary operator is not applicable for derivative");
        }
    }

    private bool IsEqual(Expression e, double value)
    {
        if (!(e is ConstantExpression eConst))
            return false;

        if (eConst.Type != typeof(double))
            return false;

        Debug.Assert(eConst.Value != null);

        return (double)eConst.Value == value;
    }

    private Expression Add(Expression a, Expression b)
    {
        if (IsEqual(a, 0.0))
            return b;

        if (IsEqual(b, 0.0))
            return a;

        return Expression.Add(a, b);
    }

    private Expression Multiply(Expression a, Expression b)
    {
        if (IsEqual(a, 0.0))
            return Expression.Constant(0.0);

        if (IsEqual(b, 0.0))
            return Expression.Constant(0.0);

        if (IsEqual(a, 1.0))
            return b;

        if (IsEqual(b, 1.0))
            return a;

        return Expression.Multiply(a, b);
    }
}

// SySharp
// https://www.nuget.org/packages/SySharp/
// https://github.com/markshevchenko/sysharp
