// Copyright (c) 2007-2017 Thong Nguyen (tumtumtum@gmail.com)
namespace Shaolinq.Tests.TestModel
{
	[DataAccessObject]
	public abstract class ObjectWithDaoPrimaryKey
		: DataAccessObject<ObjectWithManyTypes>
	{
		[PersistedMember]
		public abstract string Something { get; set; }
	}
}