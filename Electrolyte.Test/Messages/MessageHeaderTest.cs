using System;
using System.IO;
using NUnit.Framework;
using Electrolyte.Messages;

namespace Electrolyte.Test.Messages {
	[TestFixture]
	public class MessageHeaderTest {
		[Test]
		public void Read() {
			Tuple<byte[], string, uint, byte[]>[] Headers = new Tuple<byte[], string, uint, byte[]>[] {
				new Tuple<byte[], string, uint, byte[]>(
					new byte[]{0xF9,0xBE,0xB4,0xD9,0x76,0x65,0x72,0x73,0x69,0x6F,0x6E,0x00,0x00,0x00,0x00,0x00,0x64,0x00,0x00,0x00,0x35,0x8D,0x49,0x32},
					"version", 100, new byte[]{0x35, 0x8D, 0x49, 0x32}
				)
			};

			byte[] badMagic = {
				0xF9, 0xBE, 0xB4, 0xE0,
				0x76, 0x65, 0x72, 0x73, 0x69, 0x6F, 0x6E, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x64, 0x00, 0x00, 0x00,
				0x35, 0x8D, 0x49, 0x32,
			};

			byte[] badSize = {
				0xF9, 0xBE, 0xB4, 0xD9,
				0x76, 0x65, 0x72, 0x73, 0x69, 0x6F, 0x6E, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x64, 0x00, 0x00, 0x00,
				0x35, 0x8D, 0x49
			};

			foreach(var header in Headers) {
				using(BinaryReader reader = new BinaryReader(new MemoryStream(header.Item1))) {
					MessageHeader msgHeader = MessageHeader.Read(reader);
					Assert.AreEqual(header.Item2, msgHeader.Command);
					Assert.AreEqual(header.Item3, msgHeader.PayloadLength);
					Assert.AreEqual(header.Item4, msgHeader.ExpectedChecksum);
				}
			}


			using(BinaryReader reader = new BinaryReader(new MemoryStream(badMagic)))
				Assert.Throws<InvalidHeaderException>(() => { MessageHeader.Read(reader); });

			using(BinaryReader reader = new BinaryReader(new MemoryStream(badSize)))
				Assert.Throws<InvalidHeaderException>(() => { MessageHeader.Read(reader); });
		}

		[Test]
		public void Write() {
			using(MemoryStream stream = new MemoryStream())
			using(BinaryWriter writer = new BinaryWriter(stream)) {
				byte[] expected = {
					0xF9, 0xBE, 0xB4, 0xD9,													// Magic
					0x76, 0x65, 0x72, 0x73, 0x69, 0x6F, 0x6E, 0x00, 0x00, 0x00, 0x00, 0x00, // Message ("version     ")
					0x64, 0x00, 0x00, 0x00,													// Payload length (100 bytes)
					0x35, 0x8D, 0x49, 0x32													// Payload checksum
				};

				MessageHeader header = new MessageHeader("version", 100, new byte[]{0x35, 0x8D, 0x49, 0x32, 0xAA});

				header.Write(writer);
				Assert.AreEqual(expected, stream.ToArray());
			}
		}
	}
}

