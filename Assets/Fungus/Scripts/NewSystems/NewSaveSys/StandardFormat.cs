using System.Collections.Generic;

namespace CGT.Globalization
{
	public static class StandardFormatValues
	{
		public static readonly Dictionary<StandardFormat, string> vals = 
			new Dictionary<StandardFormat, string>()
			{
				{ StandardFormat.shortDate, "d" },
				{ StandardFormat.longDate, "D" },

				{ StandardFormat.fullShortDate, "f" },
				{ StandardFormat.fullLongDate, "F" },

				{ StandardFormat.generalShortDate, "g" },
				{ StandardFormat.generalLongDate, "G" },

				{ StandardFormat.monthAndDay, "m" },

				{ StandardFormat.roundTripDate, "o" },

				{ StandardFormat.RFC1123patternDate, "r" },

				{ StandardFormat.sortableDate, "s" },

				{ StandardFormat.shortTime, "t" },
				{ StandardFormat.longTime, "T" },

				{ StandardFormat.universalSortableDate, "u" },
				{ StandardFormat.universalFullDate, "U" },

				{ StandardFormat.yearAndMonth, "y" }
			};

	}

	public enum StandardFormat
	{
		shortDate,
		longDate,

		fullShortDate,
		fullLongDate,

		generalShortDate,
		generalLongDate,

		monthAndDay,

		roundTripDate,

		RFC1123patternDate,

		sortableDate,

		shortTime,
		longTime,

		universalSortableDate,
		universalFullDate,

		yearAndMonth,
	}
}