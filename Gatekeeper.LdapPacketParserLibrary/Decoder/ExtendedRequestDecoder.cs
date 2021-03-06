using System.Formats.Asn1;
using Gatekeeper.LdapPacketParserLibrary.Models.Operations.Request;

namespace Gatekeeper.LdapPacketParserLibrary.Decoder
{
    internal class ExtendedRequestDecoder : IApplicationDecoder<ExtendedRequest>
    {
        public ExtendedRequest TryDecode(AsnReader reader, byte[] input)
        {
            Asn1Tag bindRequestApplication = new Asn1Tag(TagClass.Application, 23);
            AsnReader subReader = reader.ReadSequence(bindRequestApplication);
            Asn1Tag contextTag = new Asn1Tag(TagClass.ContextSpecific, 0);

            string requestName = System.Text.Encoding.ASCII.GetString(subReader.ReadOctetString(contextTag));
            string? requestValue = null;
            if (subReader.HasData)
            {
                requestValue = System.Text.Encoding.ASCII.GetString(subReader.ReadOctetString(contextTag));
            }

            return new ExtendedRequest
            {
                RequestName = requestName,
                RequestValue = requestValue,
            };
        }
    }
}
