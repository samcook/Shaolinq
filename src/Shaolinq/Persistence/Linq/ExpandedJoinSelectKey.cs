﻿// Copyright (c) 2007-2017 Thong Nguyen (tumtumtum@gmail.com)
namespace Shaolinq.Persistence.Linq
{
	internal struct ExpandedJoinSelectKey<L, R>
	{
		public L Outer { get; set; }
		public R Inner { get; set; }
	}
}