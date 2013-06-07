using System;
using System.IO;
using NUnit.Framework;
using Electrolyte.Messages;

namespace Electrolyte.Test.Messages {
	[TestFixture]
	public class MessageHeaderTest {
		[Test]
		public void TestMagicBytes() {
			byte[] goodHeader = {
				0xF9, 0xBE, 0xB4, 0xD9,													// Magic
				0x76, 0x65, 0x72, 0x73, 0x69, 0x6F, 0x6E, 0x00, 0x00, 0x00, 0x00, 0x00, // Message ("version     ")
				0x64, 0x00, 0x00, 0x00,													// Payload length (100 bytes)
				0x35, 0x8D, 0x49, 0x32													// Payload checksum
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

			using(BinaryReader reader = new BinaryReader(new MemoryStream(goodHeader))) {
				MessageHeader header = new MessageHeader(reader);
				Assert.AreEqual(header.Command, "version");
				Assert.AreEqual(header.PayloadLength, 100);
				Assert.AreEqual(header.ExpectedChecksum, new byte[] {0x35, 0x8D, 0x49, 0x32});
			}

			using(BinaryReader reader = new BinaryReader(new MemoryStream(badMagic)))
				Assert.Throws<InvalidHeaderException>(() => { new MessageHeader(reader); });

			using(BinaryReader reader = new BinaryReader(new MemoryStream(badSize)))
				Assert.Throws<InvalidHeaderException>(() => { new MessageHeader(reader); });
		}
	}
}

