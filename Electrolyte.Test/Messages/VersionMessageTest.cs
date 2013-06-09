using System;
using System.IO;
using System.Net;
using NUnit.Framework;
using Electrolyte.Messages;

namespace Electrolyte.Test.Messages {
	[TestFixture]
	public class VersionMessageTest {
		[Test]
		public void TestVersionInfo() {
			byte[] bytes = {
				0xF9, 0xBE, 0xB4, 0xD9,																																		// Magic
				0x76, 0x65, 0x72, 0x73, 0x69, 0x6F, 0x6E, 0x00, 0x00, 0x00, 0x00, 0x00, 																					// Message ("version     ")
				0x64, 0x00, 0x00, 0x00,																																		// Payload length (100 bytes)
				0x35, 0x8D, 0x49, 0x32,																																		// Payload checksum

				0x62, 0xEA, 0x00, 0x00,																																		// Version 60002
				0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,																												// Services.NodeNetwork
				0x11, 0xB2, 0xD0, 0x50, 0x00, 0x00, 0x00, 0x00,																												// 2012 Dec 18 05:12:33 UTC
				0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x0A, 0x00, 0x00, 0x01, 0x20, 0x8D,	// Recipient (::ffff:10.0.0.1 @ 8333)
				0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x20, 0x01, 0x0D, 0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0x00, 0x00, 0x42, 0x83, 0x29, 0x69, 0x87,	// Sender (2001:db8::ff00:42:8329 @ 27015)
				0x3B, 0x2E, 0xB3, 0x5D, 0x8C, 0xE6, 0x17, 0x65,																												// Random unique ID (nonce)
				0x0F, 0x2F, 0x53, 0x61, 0x74, 0x6F, 0x73, 0x68, 0x69, 0x3A, 0x30, 0x2E, 0x37, 0x2E, 0x32, 0x2F,																// User-agent ("/Satoshi:0.7.2/")
				0xC0, 0x3E, 0x03, 0x00																																		// Latest block height (#212672)
			};

			using(BinaryReader reader = new BinaryReader(new MemoryStream(bytes))) {
				VersionMessage version = VersionMessage.Read(reader);

				Assert.AreEqual(version.Version, 60002);
				Assert.AreEqual(version.AvailableServices, VersionMessage.Services.NodeNetwork);
				Assert.AreEqual(version.Time, new DateTime(2012, 12, 18, 18, 12, 33, DateTimeKind.Utc));

				Assert.AreEqual(version.Receiver.AvailableServices, VersionMessage.Services.NodeNetwork);
				Assert.AreEqual(version.Receiver.Address, IPAddress.Parse("10.0.0.1"));
				Assert.AreEqual(version.Receiver.Port, 8333);

				Assert.AreEqual(version.Sender.AvailableServices, VersionMessage.Services.NodeNetwork);
				Assert.AreEqual(version.Sender.Address, IPAddress.Parse("2001:db8::ff00:42:8329"));
				Assert.AreEqual(version.Sender.Port, 27015);

				Assert.AreEqual(version.UserAgent, "/Satoshi:0.7.2/");
				Assert.AreEqual(version.BlockHeight, 212672);
			}
		}
	}
}

