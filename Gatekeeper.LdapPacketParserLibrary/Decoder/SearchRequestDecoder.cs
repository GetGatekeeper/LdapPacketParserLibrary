using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Numerics;
using Gatekeeper.LdapPacketParserLibrary.Models.Operations.Request;
using static Gatekeeper.LdapPacketParserLibrary.Models.Operations.Request.SearchRequest;

namespace Gatekeeper.LdapPacketParserLibrary.Decoder
{
    internal class SearchRequestDecoder : IApplicationDecoder<SearchRequest>
    {
        public SearchRequest TryDecode(AsnReader reader, byte[] input)
        {
            SearchRequest searchRequest = new SearchRequest
            {
                RawPacket = input,
            };

            Asn1Tag bindRequestApplication = new Asn1Tag(TagClass.Application, 3);
            AsnReader subReader = reader.ReadSequence(bindRequestApplication);
            searchRequest.BaseObject = System.Text.Encoding.ASCII.GetString(subReader.ReadOctetString());
            SearchRequest.ScopeEnum scope = subReader.ReadEnumeratedValue<SearchRequest.ScopeEnum>();
            SearchRequest.DerefAliasesEnum deref = subReader.ReadEnumeratedValue<SearchRequest.DerefAliasesEnum>();
            BigInteger sizeLimit = subReader.ReadInteger();
            BigInteger timeLimit = subReader.ReadInteger();
            bool typesOnly = subReader.ReadBoolean();

            searchRequest.Filter = DecodeSearchFilter(subReader);

            return searchRequest;
        }

        private TFilter DecodeAttributeValueAssertionFilter<TFilter>(AsnReader reader) where TFilter : AttributeValueAssertionFilter, new()
        {
            AsnReader subReader = reader.ReadSequence(new Asn1Tag(TagClass.ContextSpecific, reader.PeekTag().TagValue));
            string attributeDescription = System.Text.Encoding.ASCII.GetString(subReader.ReadOctetString());
            string assertionValue = System.Text.Encoding.ASCII.GetString(subReader.ReadOctetString());

            return new TFilter { AssertionValue = assertionValue, AttributeDesc = attributeDescription };
        }

        private List<IFilterChoice> DecodeRecursiveFilterSets(AsnReader reader)
        {
            AsnReader subReader = reader.ReadSetOf(new Asn1Tag(TagClass.ContextSpecific, reader.PeekTag().TagValue));
            List<IFilterChoice> filters = new List<IFilterChoice>();

            while (subReader.HasData)
            {
                filters.Add(DecodeSearchFilter(subReader));
            }

            return filters;
        }

        private SubstringFilter DecodeSubstringFilter(AsnReader reader)
        {
            AsnReader subReader = reader.ReadSequence(new Asn1Tag(TagClass.ContextSpecific, 4));

            string attributeDescription = System.Text.Encoding.ASCII.GetString(subReader.ReadOctetString());

            SubstringFilter filter = new SubstringFilter
            {
                AttributeDesc = attributeDescription,
            };

            AsnReader substringSequenceReader = subReader.ReadSequence();
            while (substringSequenceReader.HasData)
            {
                switch (substringSequenceReader.PeekTag().TagValue)
                {
                    case 0:
                        filter.Initial = System.Text.Encoding.ASCII.GetString(substringSequenceReader.ReadOctetString(new Asn1Tag(TagClass.ContextSpecific, 0)));
                        break;
                    case 1:
                        filter.Any.Add(System.Text.Encoding.ASCII.GetString(substringSequenceReader.ReadOctetString(new Asn1Tag(TagClass.ContextSpecific, 1))));
                        break;
                    case 2:
                        filter.Final = System.Text.Encoding.ASCII.GetString(substringSequenceReader.ReadOctetString(new Asn1Tag(TagClass.ContextSpecific, 2)));
                        break;
                }
            }

            return filter;
        }

        private IFilterChoice DecodeSearchFilter(AsnReader reader)
        {
            switch (reader.PeekTag().TagValue)
            {
                case 0:
                    return new AndFilter { Filters = DecodeRecursiveFilterSets(reader) };
                case 1:
                    return new OrFilter { Filters = DecodeRecursiveFilterSets(reader) };
                case 2:
                    return new NotFilter { Filter = DecodeSearchFilter(reader) };
                case 3:
                    return DecodeAttributeValueAssertionFilter<EqualityMatchFilter>(reader);
                case 4:
                    return DecodeSubstringFilter(reader);
                case 5:
                    return DecodeAttributeValueAssertionFilter<GreaterOrEqualFilter>(reader);
                case 6:
                    return DecodeAttributeValueAssertionFilter<LessOrEqualFilter>(reader);
                case 7:
                    return new PresentFilter { Value = System.Text.Encoding.ASCII.GetString(reader.ReadOctetString(new Asn1Tag(TagClass.ContextSpecific, reader.PeekTag().TagValue))) };
                case 8:
                    return DecodeAttributeValueAssertionFilter<ApproxMatchFilter>(reader);
                default:
                    throw new NotImplementedException("Cannot decode the tag: " + reader.PeekTag().TagValue);
            }
        }
    }
}
