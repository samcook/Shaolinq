// Copyright (c) 2007-2017 Thong Nguyen (tumtumtum@gmail.com)

using System.Linq.Expressions;

namespace Shaolinq
{
	public interface IHasCondition
	{
		LambdaExpression Condition { get; }
	}
}