using System.Collections.Generic;
using System;
using System.Text;

public enum ServerPackets
{
    WelcomePlayer = 1,
    PlayerLeave,

    RegistrationFailed,
    RegistrationSuccessful,

    SendPlayerIntoGame,
    RemovePlayerFromGame,
    UpdatePlayerOrder,
    CountdownStartGame,

    SetupGame,
    StartRound,
    SendQuestion,
    SendAnswer,
    SkipQuiz,
    WaitForNextPlayer,
    PickNextPlayer,
    VerifyAnswer,
    ShowResult,
    EndRound,
    EndGame,
    UpdateRoundInfo
}

public enum ClientPackets
{
    HandshakeServer = 1,
    ResendUsername,
    GiveAnswer,
    Ready,
    GiveQuestionToOther,
    Skip,
}

public class Packet : IDisposable
{
    private List<Byte> _buffer;
    private Byte[] _readableBuffer;
    private int _readPointer;
    private bool _disposed = false;

    public int Length
    {
        get { return _buffer.Count; }
        private set { }
    }

    public Byte[] Array
    {
        get
        {
            _readableBuffer = _buffer.ToArray();
            return _readableBuffer;
        }
        private set { }
    }

    public int UnreadLength
    {
        get { return Length - _readPointer; ; }
        private set { }
    }

    public Packet()
    {
        _buffer = new List<Byte>();
        _readPointer = 0;
    }
    public Packet(int id)
    {
        _buffer = new List<Byte>();
        _readPointer = 0;

        PutInt(id);
    }

    public Packet(Byte[] data)
    {
        _buffer = new List<byte>();
        _readPointer = 0;

        AssignBytes(data);
    }

    public void AssignBytes(Byte[] data)
    {
        PutBytes(data);
        _readableBuffer = _buffer.ToArray();
    }

    // Length is always at the beginning of the packet
    public void InsertLength()
    {
        _buffer.InsertRange(0, BitConverter.GetBytes(_buffer.Count));
    }

    public void InsertInt(int value)
    {
        _buffer.InsertRange(0, BitConverter.GetBytes(value));
    }

    // Reset the packet (delete all contents)
    public void Reset()
    {
        _buffer.Clear();
        _readableBuffer = null;
        _readPointer = 0;
    }

    // Unread the last integer
    public void Revert()
    {
        _readPointer -= 4;
    }

    #region Write Data
    public void PutByte(Byte value) => _buffer.Add(value);

    public void PutBytes(Byte[] value) => _buffer.AddRange(value);

    public void PutShort(short value) => _buffer.AddRange(BitConverter.GetBytes(value));

    public void PutInt(int value) => _buffer.AddRange(BitConverter.GetBytes(value));

    public void PutLong(long value) => _buffer.AddRange(BitConverter.GetBytes(value));

    public void PutFloat(float value) => _buffer.AddRange(BitConverter.GetBytes(value));

    public void PutBool(bool value) => _buffer.AddRange(BitConverter.GetBytes(value));

    public void PutString(string value)
    {
        // a string does not have a fixed size, so we write the length before every string
        PutInt(value.Length);
        _buffer.AddRange(Encoding.ASCII.GetBytes(value));
    }
    #endregion

    #region Read Data
    public Byte ReadByte(bool moveNext = true)
    {
        if (_buffer.Count > _readPointer)
        {
            Byte value = _readableBuffer[_readPointer];
            if (moveNext) _readPointer += sizeof(Byte);
            return value;
        }
        else throw new Exception("Read byte error!");
    }

    public Byte[] ReadBytes(int length, bool moveNext = true)
    {
        if (_buffer.Count > _readPointer)
        {
            Byte[] value = _buffer.GetRange(_readPointer, length).ToArray();
            if (moveNext) _readPointer += length;
            return value;
        }
        else throw new Exception("Read byte array error!");
    }

    public short ReadShort(bool moveNext = true)
    {
        if (_buffer.Count > _readPointer)
        {
            short value = BitConverter.ToInt16(_readableBuffer, _readPointer);
            if (moveNext) _readPointer += sizeof(short);
            return value;
        }
        else throw new Exception("Failed to read short");
    }

    public int ReadInt(bool moveNext = true)
    {
        if (_buffer.Count > _readPointer)
        {
            int _value = BitConverter.ToInt32(_readableBuffer, _readPointer);
            if (moveNext) _readPointer += sizeof(int);
            return _value;
        }
        else throw new Exception("Failed to read int");
    }

    public long ReadLong(bool moveNext = true)
    {
        if (_buffer.Count > _readPointer)
        {
            long _value = BitConverter.ToInt64(_readableBuffer, _readPointer);
            if (moveNext) _readPointer += sizeof(long);
            return _value;
        }
        else throw new Exception("Failed to read long");
    }

    public float ReadFloat(bool moveNext = true)
    {
        if (_buffer.Count > _readPointer)
        {
            float _value = BitConverter.ToSingle(_readableBuffer, _readPointer);
            if (moveNext) _readPointer += sizeof(float);
            return _value;
        }
        else
        {
            throw new Exception("Failed to read float");
        }
    }

    public bool ReadBool(bool moveNext = true)
    {
        if (_buffer.Count > _readPointer)
        {
            bool _value = BitConverter.ToBoolean(_readableBuffer, _readPointer);
            if (moveNext) _readPointer += sizeof(bool);
            return _value;
        }
        else
        {
            throw new Exception("Failed to read bool");
        }
    }

    public string ReadString(bool moveNext = true)
    {
        try
        {
            int length = ReadInt(); // Get the length of the string
            string _value = Encoding.ASCII.GetString(_readableBuffer, _readPointer, length); // Convert the bytes to a string
            if (moveNext && _value.Length > 0) _readPointer += length;
            return _value;
        }
        catch
        {
            throw new Exception("Failed to read string");
        }
    }
    #endregion

    protected virtual void Dispose(bool isDisposing)
    {
        if (!_disposed)
        {
            if (isDisposing)
            {
                _buffer = null;
                _readableBuffer = null;
                _readPointer = 0;
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}