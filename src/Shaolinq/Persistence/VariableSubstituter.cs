// Copyright (c) 2007-2018 Thong Nguyen (tumtumtum@gmail.com)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Platform;
using Shaolinq.Persistence.Linq;

namespace Shaolinq.Persistence
{
	internal static class VariableSubstituter
	{
		private static readonly Regex PatternRegex = new Regex(@"(?<prefix>^|[^\\\$]*|[\\\\]+)(\$(?<name>[0-9])+|\$\((?<env>env\.)?(?<name>[a-z_A-Z]+?)((?<tolower>_TOLOWER)|(:(?<format>[^\)]+)))?\))", RegexOptions.Compiled | RegexOptions.IgnoreCase); 

		public static string Substitute(string value, Func<string, object> variableToValue)
		{
			return PatternRegex.Replace(value, match =>
			{
				var result = match.Groups["env"].Length != 0 ? Environment.GetEnvironmentVariable(match.Groups["name"].Value) : variableToValue(match.Groups["name"].Value);

				if (match.Groups["tolower"].Length > 0 && result is string)
				{
					result = ((string)result).ToLowerInvariant();
				}

				if (result is IEnumerable && !(result is string))
				{
					result = string.Join("_", (result as IEnumerable).ToTyped<object>()?.Select(c => c.ToString()).ToArray());
				}

				var format = match.Groups["format"].Value;

				if (format.Length > 0)
				{
					switch (format)
					{
					case "L":
						if (result is string)
						{
							result = ((string)result).ToLowerInvariant();
						}
						break;
					case "U":
						if (result is string)
						{
							result = ((string)result).ToUpperInvariant();
						}
						break;
					case "_":
						result = string.Join("_", (result as IEnumerable).ToTyped<object>()?.Select(c => c.ToString()).ToArray());
						break;
					}
				}

				return match.Groups["prefix"].Value + result;
			});
		}
		
		public static string Substitute(string input, TypeDescriptor typeDescriptor)
		{
			if (input == null)
			{
				return typeDescriptor.TypeName;
			}

			var visitedTypes = new HashSet<TypeDescriptor>();
			
			return Substitute(input, value =>
			{
				switch (value.ToUpper())
				{
				case "TYPENAME":
					return typeDescriptor.TypeName;
				case "TABLENAME":
				case "PERSISTED_TYPENAME":
					if (visitedTypes.Contains(typeDescriptor))
					{
						throw new InvalidOperationException("Recursive variable substitution");
					}
					visitedTypes.Add(typeDescriptor);
					return typeDescriptor.PersistedName;
				default:
					throw new NotSupportedException(value);
				}
			});
		}

		public static string Substitute(string input, PropertyDescriptor property)
		{
			return Substitute(input, new[] { property });
		}

		public static string Substitute(string input, PropertyDescriptor[] properties, Func<int, string> indexedToValue = null)
		{
			input = input ?? "";

			return Substitute(input, value =>
			{
				var s = value.ToUpper();

				if (properties?.Length > 0)
				{
					var property = properties[0];

					switch (s)
					{
					case "TYPENAME":
						return property.DeclaringTypeDescriptor.TypeName;
					case "PROPERTYNAME":
						return property.PropertyName;
					case "PROPERTYTYPENAME":
						return property.PropertyType.Name;
					case "PERSISTED_TYPENAME":
						return property.DeclaringTypeDescriptor.PersistedName;
					case "PERSISTED_PROPERTYNAME":
						return property.PersistedName;
					case "PERSISTED_PROPERTYTYPENAME":
						return property.PropertyTypeTypeDescriptor.PersistedName;
					case "PROPERTYNAMES":
						return properties.Select(c => c.PropertyName);
					case "PERSISTED_PROPERTYNAMES":
						return properties.Select(c => c.PersistedName);
					case "PERSISTED_PROPERTYPREFIXNAMES":
						return properties.Select(c => c.PrefixName);
					case "PERSISTED_PROPERTYSUFFIXNAMES":
						return properties.Select(c => c.SuffixName);
					case "COLUMNNAME":
						return QueryBinder.GetColumnInfos(property.DeclaringTypeDescriptor.TypeDescriptorProvider, property);
					case "COLUMNNAMES":
						return QueryBinder.GetColumnInfos(property.DeclaringTypeDescriptor.TypeDescriptorProvider, properties);
					}
				}
				
				if (indexedToValue != null && int.TryParse(s, out var number))
				{
					return indexedToValue(number);
				}

				throw new NotSupportedException(value);
			});
		}

		public static string SedTransform(string value, string transformString, PropertyDescriptor property)
		{
			return SedTransform(value, transformString, new[] { property });
		}

		private struct SedDirective
		{
			public string Pattern { get; set; }
			public string Replacement { get; set; }
			public string Options { get; set; }
		}

		private static IEnumerable<SedDirective> GetSedDirectives(string transformString)
		{
			var currentPos = 0;

			char? Consume(Func<char?, bool> keepGoing = null)
			{
				if (keepGoing == null)
				{
					if (currentPos < transformString.Length)
					{
						currentPos++;
					}

					return CurrentChar();
				}

				while (currentPos < transformString.Length && keepGoing(CurrentChar()))
				{
					currentPos++;
				}

				return CurrentChar();
			}

			char? CurrentChar()
			{
				if (currentPos >= transformString.Length)
				{
					return null;
				}

				return transformString[currentPos];
			}

			while (CurrentChar() != null)
			{
				var current = new SedDirective();

				Consume(c => c != null && char.IsWhiteSpace(c.Value));

				if (CurrentChar() != 's')
				{
					throw new ArgumentException($"Invalid string (expected 's' at {currentPos}): {transformString}", nameof(transformString));
				}

				Consume();

				var separatorChar = CurrentChar();

				var buffer = new StringBuilder();

				while (Consume() != null)
				{
					if (CurrentChar() == separatorChar)
					{
						break;
					}

					buffer.Append(CurrentChar());
				}

				if (CurrentChar() != separatorChar)
				{
					throw new ArgumentException($"Invalid string (expected '{separatorChar}' at {currentPos}): {transformString}", nameof(transformString));
				}

				current.Pattern = buffer.ToString();
				buffer.Length = 0;

				while (Consume() != null)
				{
					if (CurrentChar() == separatorChar)
					{
						break;
					}

					buffer.Append(CurrentChar());
				}

				if (CurrentChar() != separatorChar)
				{
					throw new ArgumentException($"Invalid string (expected '{separatorChar}' at {currentPos}): {transformString}", nameof(transformString));
				}

				current.Replacement = buffer.ToString();
				buffer.Length = 0;

				while (Consume() != null)
				{
					if (CurrentChar() == ' ' || CurrentChar() == ';')
					{
						break;
					}

					buffer.Append(CurrentChar());
				}

				current.Options = buffer.ToString();

				yield return current;

				Consume(c => c != null && char.IsWhiteSpace(c.Value));

				if (CurrentChar() == ';')
				{
					Consume();
					Consume(c => c != null && char.IsWhiteSpace(c.Value));

					continue;
				}

				break;
			}
		}

		public static string SedTransform(string value, string transformString, PropertyDescriptor[] properties = null)
		{
			if (string.IsNullOrEmpty(transformString))
			{
				return value;
			}

			value = value ?? "";
			
			foreach (var directive in GetSedDirectives(transformString))
			{
				if (transformString.Length < 4 || transformString[0] != 's')
				{
					throw new ArgumentException(nameof(transformString));
				}

				var pattern = directive.Pattern;
				var replacement = directive.Replacement;
				var options = directive.Options;
				var useReplacementRegex = replacement.Contains("$");

				var maxCount = 1;
				var regexOptions = RegexOptions.None;

				if (options.Contains("g"))
				{
					maxCount = int.MaxValue;
				}

				if (options.Contains("i"))
				{
					regexOptions |= RegexOptions.IgnoreCase;
				}

				var count = 0;

				value = Regex.Replace
				(value, pattern, match =>
				{
					if (maxCount != int.MaxValue && count > maxCount)
					{
						return match.Value;
					}

					count++;

					var result = useReplacementRegex ? Substitute(replacement, properties, index => match.Groups[index].Value) : replacement;

					return result;
				}, regexOptions);
			}

			return value;
		}
	}
}
