using System.Formats.Asn1;
using System.Numerics;
using Gatekeeper.LdapPacketParserLibrary.Models.Operations.Request;

namespace Gatekeeper.LdapPacketParserLibrary.Decoder
{
    internal class BindRequestDecoder : IApplicationDecoder<BindRequest>
    {
        public BindRequest TryDecode(AsnReader reader, byte[] input)
        {
            Asn1Tag bindRequestApplication = new Asn1Tag(TagClass.Application, 0);
            AsnReader subReader = reader.ReadSequence(bindRequestApplication);

            BigInteger version = subReader.ReadInteger();

            string nameString = System.Text.Encoding.ASCII.GetString(subReader.ReadOctetString());

            Asn1Tag authContext = new Asn1Tag(TagClass.ContextSpecific, 0);
            string authString = System.Text.Encoding.ASCII.GetString(subReader.ReadOctetString(authContext));

            subReader.ThrowIfNotEmpty();
            reader.ThrowIfNotEmpty();

            BindRequest bindRequest = new BindRequest(version, nameString, authString);

            return bindRequest;
        }
    }
}