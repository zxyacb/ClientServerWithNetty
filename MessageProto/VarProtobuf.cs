using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageProto
{
    public class VarProtobufEncoder : MessageToMessageEncoder<IMessage>
    {
        public override bool IsSharable => true;

        protected override void Encode(IChannelHandlerContext context, IMessage message, List<object> output)
        {
            IByteBuffer buffer = null;
            try
            {
                //int size = message.CalculateSize();
                //if (size == 0)
                //{
                //    return;
                //}
                //todo: Implement ByteBufferStream to avoid allocations.
                buffer = Unpooled.WrappedBuffer(VarMessageMap.GetMessageType(message), message.ToByteArray());
                output.Add(buffer);
                buffer = null;
            }
            catch (Exception exception)
            {
                throw new CodecException(exception);
            }
            finally
            {
                buffer?.Release();
            }
        }
    }

    public class VarProtobufDecoder : MessageToMessageDecoder<IByteBuffer>
    {
        public override bool IsSharable => true;

        protected override void Decode(IChannelHandlerContext context, IByteBuffer message, List<object> output)
        {
            int length = message.ReadableBytes;
            if (length <= 0)
            {
                return;
            }
            Stream inputStream = null;
            try
            {
                short messageType = message.ReadShort();
                length -= 2;
                CodedInputStream codedInputStream;
                if (message.IoBufferCount == 1)
                {
                    ArraySegment<byte> bytes = message.GetIoBuffer(message.ReaderIndex, length);
                    codedInputStream = new CodedInputStream(bytes.Array, bytes.Offset, length);
                }
                else
                {
                    inputStream = new ReadOnlyByteBufferStream(message, false);
                    codedInputStream = new CodedInputStream(inputStream);
                }

                //
                // Note that we do not dispose the input stream because there is no input stream attached.
                // Ideally, it should be disposed. BUT if it is disposed, a null reference exception is
                // thrown because CodedInputStream flag leaveOpen is set to false for direct byte array reads,
                // when it is disposed the input stream is null.
                //
                // In this case it is ok because the CodedInputStream does not own the byte data.
                //
                IMessage decoded = VarMessageMap.GetMessageParser(messageType).ParseFrom(codedInputStream);
                if (decoded != null)
                {
                    output.Add(decoded);
                }
            }
            catch (Exception exception)
            {
                throw new CodecException(exception);
            }
            finally
            {
                inputStream?.Dispose();
            }
        }
    }

    public static class VarMessageMap
    {
        static VarMessageMap()
        {
            _parser1 = new Dictionary<int, MessageParser>()
            {
                {1, HeartBeatMessage.Parser },
                {2, BasicMessage.Parser }
            };
            _parser2 = new Dictionary<MessageParser, int>();
            foreach (var kv in _parser1)
            {
                _parser2.Add(kv.Value, kv.Key);
            }
        }
        private readonly static Dictionary<int, MessageParser> _parser1;
        private readonly static Dictionary<MessageParser, int> _parser2;
        public static MessageParser GetMessageParser(int type)
        {
            if (_parser1.ContainsKey(type)) return _parser1[type];
            else throw new Exception("未定义解析类型");
        }
        public static byte[] GetMessageType(IMessage message)
        {
            int type = _parser2.ContainsKey(message.Descriptor.Parser) ? _parser2[message.Descriptor.Parser] : throw new Exception("未定义解析类型");
            return new byte[2] { (byte)((type >> 8 & 0xff)), (byte)(type & 0xff) };
        }
    }
}
