// Copyright (c) 2007-2013 Thong Nguyen (tumtumtum@gmail.com)

﻿using System;

namespace Shaolinq
{
	public class MutableTuple<T1>
	{
		public T1 Item1 { get; set; }

		public MutableTuple()
		{
		}
		
		public MutableTuple(T1 item1)
		{
			this.Item1 = item1;
		}
	}

	public class MutableTuple<T1, T2>
	{
		public T1 Item1 { get; set; }
		public T2 Item2 { get; set; }

		public MutableTuple()
		{	
		}

		public MutableTuple(T1 item1, T2 item2)
		{
			this.Item1 = item1;
			this.Item2 = item2;
		}
	}

	public class MutableTuple<T1, T2, T3>
	{
		public T1 Item1 { get; set; }
		public T2 Item2 { get; set; }
		public T3 Item3 { get; set; }

		public MutableTuple()
		{
		}

		public MutableTuple(T1 item1, T2 item2, T3 item3)
		{
			this.Item1 = item1;
			this.Item2 = item2;
			this.Item3 = item3;
		}
	}

	public class MutableTuple<T1, T2, T3, T4>
	{
		public T1 Item1 { get; set; }
		public T2 Item2 { get; set; }
		public T3 Item3 { get; set; }
		public T4 Item4 { get; set; }

		public MutableTuple()
		{
		}

		public MutableTuple(T1 item1, T2 item2, T3 item3, T4 item4)
		{
			this.Item1 = item1;
			this.Item2 = item2;
			this.Item3 = item3;
			this.Item4 = item4;
		}
	}

	public class MutableTuple<T1, T2, T3, T4, T5>
	{
		public T1 Item1 { get; set; }
		public T2 Item2 { get; set; }
		public T3 Item3 { get; set; }
		public T4 Item4 { get; set; }
		public T5 Item5 { get; set; }

		public MutableTuple()
		{
		}

		public MutableTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
		{
			this.Item1 = item1;
			this.Item2 = item2;
			this.Item3 = item3;
			this.Item4 = item4;
			this.Item5 = item5;
		}
	}

	public class MutableTuple<T1, T2, T3, T4, T5, T6>
	{
		public T1 Item1 { get; set; }
		public T2 Item2 { get; set; }
		public T3 Item3 { get; set; }
		public T4 Item4 { get; set; }
		public T5 Item5 { get; set; }
		public T6 Item6 { get; set; }

		public MutableTuple()
		{
		}

		public MutableTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
		{
			this.Item1 = item1;
			this.Item2 = item2;
			this.Item3 = item3;
			this.Item4 = item4;
			this.Item5 = item5;
			this.Item6 = item6;
		}
	}

	public class MutableTuple<T1, T2, T3, T4, T5, T6, T7>
	{
		public T1 Item1 { get; set; }
		public T2 Item2 { get; set; }
		public T3 Item3 { get; set; }
		public T4 Item4 { get; set; }
		public T5 Item5 { get; set; }
		public T6 Item6 { get; set; }
		public T7 Item7 { get; set; }

		public MutableTuple()
		{
		}

		public MutableTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7)
		{
			this.Item1 = item1;
			this.Item2 = item2;
			this.Item3 = item3;
			this.Item4 = item4;
			this.Item5 = item5;
			this.Item6 = item6;
			this.Item7 = item7;
		}
	}

	public class MutableTuple<T1, T2, T3, T4, T5, T6, T7, T8>
	{
		public T1 Item1 { get; set; }
		public T2 Item2 { get; set; }
		public T3 Item3 { get; set; }
		public T4 Item4 { get; set; }
		public T5 Item5 { get; set; }
		public T6 Item6 { get; set; }
		public T7 Item7 { get; set; }
		public T8 Item8 { get; set; }

		public MutableTuple()
		{
		}

		public MutableTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, T8 item8)
		{
			this.Item1 = item1;
			this.Item2 = item2;
			this.Item3 = item3;
			this.Item4 = item4;
			this.Item5 = item5;
			this.Item6 = item6;
			this.Item7 = item7;
			this.Item8 = item8;
		}
	}
}