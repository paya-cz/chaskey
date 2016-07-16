using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Chaskey.Tests
{
    [TestClass]
    public class ChaskeyTest
    {
        // Test battery taken from: http://mouha.be/wp-content/uploads/chaskey-speed.c

        [TestMethod]
        public void TestBattery()
        {
            // 128-bit key
            var key = new uint[] { 0x833D3433, 0x009F389F, 0x2398E64F, 0x417ACF39 }.SelectMany(x => BitConverter.GetBytes(x)).ToArray();

            // Chaskey initialized with the key
            var prf = new Chaskey(key, 0, key.Length);

            // Perform the test battery
            var message = new byte[64];
            for (int i = 0; i < 64; i++)
            {
                message[i] = (byte)i;

                // Compute the tag
                var tag = prf.Compute(message, 0, i);
                // Get the target tag
                var targetTag = vectors[i].SelectMany(v => BitConverter.GetBytes(v));

                if (tag.SequenceEqual(targetTag) == false)
                    throw new Exception(string.Format("Test vector failed for {0:N}-byte message!", i));
            }
        }


        private static readonly uint[][] vectors = new[] {
            new [] { 0x792E8FE5u, 0x75CE87AAu, 0x2D1450B5u, 0x1191970Bu },
            new [] { 0x13A9307Bu, 0x50E62C89u, 0x4577BD88u, 0xC0BBDC18u },
            new [] { 0x55DF8922u, 0x2C7FF577u, 0x73809EF4u, 0x4E5084C0u },
            new [] { 0x1BDBB264u, 0xA07680D8u, 0x8E5B2AB8u, 0x20660413u },
            new [] { 0x30B2D171u, 0xE38532FBu, 0x16707C16u, 0x73ED45F0u },
            new [] { 0xBC983D0Cu, 0x31B14064u, 0x234CD7A2u, 0x0C92BBF9u },
            new [] { 0x0DD0688Au, 0xE131756Cu, 0x94C5E6DEu, 0x84942131u },
            new [] { 0x7F670454u, 0xF25B03E0u, 0x19D68362u, 0x9F4D24D8u },
            new [] { 0x09330F69u, 0x62B5DCE0u, 0xA4FBA462u, 0xF20D3C12u },
            new [] { 0x89B3B1BEu, 0x95B97392u, 0xF8444ABFu, 0x755DADFEu },
            new [] { 0xAC5B9DAEu, 0x6CF8C0ACu, 0x56E7B945u, 0xD7ECF8F0u },
            new [] { 0xD5B0DBECu, 0xC1692530u, 0xD13B368Au, 0xC0AE6A59u },
            new [] { 0xFC2C3391u, 0x285C8CD5u, 0x456508EEu, 0xC789E206u },
            new [] { 0x29496F33u, 0xAC62D558u, 0xE0BAD605u, 0xC5A538C6u },
            new [] { 0xBF668497u, 0x275217A1u, 0x40C17AD4u, 0x2ED877C0u },
            new [] { 0x51B94DA4u, 0xEFCC4DE8u, 0x192412EAu, 0xBBC170DDu },
            new [] { 0x79271CA9u, 0xD66A1C71u, 0x81CA474Eu, 0x49831CADu },
            new [] { 0x048DA968u, 0x4E25D096u, 0x2D6CF897u, 0xBC3959CAu },
            new [] { 0x0C45D380u, 0x2FD09996u, 0x31F42F3Bu, 0x8F7FD0BFu },
            new [] { 0xD8153472u, 0x10C37B1Eu, 0xEEBDD61Du, 0x7E3DB1EEu },
            new [] { 0xFA4CA543u, 0x0D75D71Eu, 0xAF61E0CCu, 0x0D650C45u },
            new [] { 0x808B1BCAu, 0x7E034DE0u, 0x6C8B597Fu, 0x3FACA725u },
            new [] { 0xC7AFA441u, 0x95A4EFEDu, 0xC9A9664Eu, 0xA2309431u },
            new [] { 0x36200641u, 0x2F8C1F4Au, 0x27F6A5DEu, 0x469D29F9u },
            new [] { 0x37BA1E35u, 0x43451A62u, 0xE6865591u, 0x19AF78EEu },
            new [] { 0x86B4F697u, 0x93A4F64Fu, 0xCBCBD086u, 0xB476BB28u },
            new [] { 0xBE7D2AFAu, 0xAC513DE7u, 0xFC599337u, 0x5EA03E3Au },
            new [] { 0xC56D7F54u, 0x3E286A58u, 0x79675A22u, 0x099C7599u },
            new [] { 0x3D0F08EDu, 0xF32E3FDEu, 0xBB8A1A8Cu, 0xC3A3FEC4u },
            new [] { 0x2EC171F8u, 0x33698309u, 0x78EFD172u, 0xD764B98Cu },
            new [] { 0x5CECEEACu, 0xA174084Cu, 0x95C3A400u, 0x98BEE220u },
            new [] { 0xBBDD0C2Du, 0xFAB6FCD9u, 0xDCCC080Eu, 0x9F04B41Fu },
            new [] { 0x60B3F7AFu, 0x37EEE7C8u, 0x836CFD98u, 0x782CA060u },
            new [] { 0xDF44EA33u, 0xB0B2C398u, 0x0583CE6Fu, 0x846D823Eu },
            new [] { 0xC7E31175u, 0x6DB4E34Du, 0xDAD60CA1u, 0xE95ABA60u },
            new [] { 0xE0DC6938u, 0x84A0A7E3u, 0xB7F695B5u, 0xB46A010Bu },
            new [] { 0x1CEB6C66u, 0x3535F274u, 0x839DBC27u, 0x80B4599Cu },
            new [] { 0xBBA106F4u, 0xD49B697Cu, 0xB454B5D9u, 0x2B69E58Bu },
            new [] { 0x5AD58A39u, 0xDFD52844u, 0x34973366u, 0x8F467DDCu },
            new [] { 0x67A67B1Fu, 0x3575ECB3u, 0x1C71B19Du, 0xA885C92Bu },
            new [] { 0xD5ABCC27u, 0x9114EFF5u, 0xA094340Eu, 0xA457374Bu },
            new [] { 0xB559DF49u, 0xDEC9B2CFu, 0x0F97FE2Bu, 0x5FA054D7u },
            new [] { 0x2ACA7229u, 0x99FF1B77u, 0x156D66E0u, 0xF7A55486u },
            new [] { 0x565996FDu, 0x8F988CEFu, 0x27DC2CE2u, 0x2F8AE186u },
            new [] { 0xBE473747u, 0x2590827Bu, 0xDC852399u, 0x2DE46519u },
            new [] { 0xF860AB7Du, 0x00F48C88u, 0x0ABFBB33u, 0x91EA1838u },
            new [] { 0xDE15C7E1u, 0x1D90EFF8u, 0xABC70129u, 0xD9B2F0B4u },
            new [] { 0xB3F0A2C3u, 0x775539A7u, 0x6CAA3BC1u, 0xD5A6FC7Eu },
            new [] { 0x127C6E21u, 0x6C07A459u, 0xAD851388u, 0x22E8BF5Bu },
            new [] { 0x08F3F132u, 0x57B587E3u, 0x087AD505u, 0xFA070C27u },
            new [] { 0xA826E824u, 0x3F851E6Au, 0x9D1F2276u, 0x7962AD37u },
            new [] { 0x14A6A13Au, 0x469962FDu, 0x914DB278u, 0x3A9E8EC2u },
            new [] { 0xFE20DDF7u, 0x06505229u, 0xF9C9F394u, 0x4361A98Du },
            new [] { 0x1DE7A33Cu, 0x37F81C96u, 0xD9B967BEu, 0xC00FA4FAu },
            new [] { 0x5FD01E9Au, 0x9F2E486Du, 0x93205409u, 0x814D7CC2u },
            new [] { 0xE17F5CA5u, 0x37D4BDD0u, 0x1F408335u, 0x43B6B603u },
            new [] { 0x817CEEAEu, 0x796C9EC0u, 0x1BB3DED7u, 0xBAC7263Bu },
            new [] { 0xB7827E63u, 0x0988FEA0u, 0x3800BD91u, 0xCF876B00u },
            new [] { 0xF0248D4Bu, 0xACA7BDC8u, 0x739E30F3u, 0xE0C469C2u },
            new [] { 0x67363EB6u, 0xFAE8E047u, 0xF0C1C8E5u, 0x828CCD47u },
            new [] { 0x3DBD1D15u, 0x05092D7Bu, 0x216FC6E3u, 0x446860FBu },
            new [] { 0xEBF39102u, 0x8F4C1708u, 0x519D2F36u, 0xC67C5437u },
            new [] { 0x89A0D454u, 0x9201A282u, 0xEA1B1E50u, 0x1771BEDCu },
            new [] { 0x9047FAD7u, 0x88136D8Cu, 0xA488286Bu, 0x7FE9352Cu }
        };
    }
}
