using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

public static class Docker
{
    private const int BufferSize = 64 * 1024; // 64KB

    public static void Create(string filePath)
    {
        // Check if the file exists
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Error: File '{filePath}' does not exist");
            return;
        }

        // Determine the file extension
        var extension = Path.GetExtension(filePath);
        if (extension != ".cs" && extension != ".java" && extension != ".lua")
        {
            Console.WriteLine($"Error: File extension '{extension}' is not supported");
            return;
        }

        // Read the file contents
        var fileBytes = File.ReadAllBytes(filePath);

        // Compile the file
        var compiler = GetCompiler(extension);
        var compiledBytes = compiler(fileBytes);

        // Compress the file
        var compressedBytes = Compress(compiledBytes);

        // Encrypt the file
        var encryptedBytes = Encrypt(compressedBytes);

        // Save the encrypted file as a .module file
        var moduleFilePath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + ".module");
        File.WriteAllBytes(moduleFilePath, encryptedBytes);
        Console.WriteLine($"Module file created: {moduleFilePath}");
    }

    private static Func<byte[], byte[]> GetCompiler(string extension)
    {
        switch (extension)
        {
            case ".cs":
                return CompileCs;
            case ".java":
                return CompileJava;
            case ".lua":
                return CompileLua;
            default:
                throw new Exception($"Unsupported extension: {extension}");
        }
    }

    private static byte[] CompileCs(byte[] fileBytes)
    {
        // TODO: Implement C# compiler logic
        Console.WriteLine("Compiling C# file...");
        return fileBytes;
    }

    private static byte[] CompileJava(byte[] fileBytes)
    {
        // TODO: Implement Java compiler logic
        Console.WriteLine("Compiling Java file...");
        return fileBytes;
    }

    private static byte[] CompileLua(byte[] fileBytes)
    {
        // TODO: Implement Lua compiler logic
        Console.WriteLine("Compiling Lua file...");
        return fileBytes;
    }

    private static byte[] Compress(byte[] inputBytes)
    {
        using var inputStream = new MemoryStream(inputBytes);
        using var outputStream = new MemoryStream();
        using var gzipStream = new GZipStream(outputStream, CompressionLevel.Optimal);
        inputStream.CopyTo(gzipStream);
        return outputStream.ToArray();
    }

    private static byte[] Encrypt(byte[] inputBytes)
    {
        using var aes = new AesManaged();
        aes.KeySize = 128; // set the key size to 128 bits
        aes.GenerateIV();
        aes.GenerateKey();
        var key = aes.Key;
        var iv = aes.IV;
        using var encryptor = aes.CreateEncryptor(key, iv);
        using var inputStream = new MemoryStream(inputBytes);
        using var outputStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(outputStream, encryptor, CryptoStreamMode.Write);
        inputStream.CopyTo(cryptoStream);
        cryptoStream.FlushFinalBlock();
        var encryptedBytes = outputStream.ToArray();
        using var outputMemoryStream = new MemoryStream();
        using var binaryWriter = new BinaryWriter(outputMemoryStream);
        binaryWriter.Write(key.Length);
        binaryWriter.Write(key);
        binaryWriter.Write(iv.Length);
        binaryWriter.Write(iv);
        binaryWriter.Write(encryptedBytes.Length);
        binaryWriter.Write(encryptedBytes);
        return outputMemoryStream.ToArray();
    }

    public static void Run(string moduleFilePath)
    {
        // Check if the file exists
        if (!File.Exists(moduleFilePath))
        {
            Console.WriteLine($"Error: File '{moduleFilePath}' does not exist");
            return;
        }

        // Read the encrypted file contents
        var encryptedBytes = File.ReadAllBytes(moduleFilePath);

        // Decrypt the file
        var decryptedBytes = Decrypt(encryptedBytes);

        // Execute the module program and capture the output
        using var outputStream = new MemoryStream();
        Console.SetOut(new StreamWriter(outputStream));
        ExecuteModule(decryptedBytes);
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()));

        // Print the output to the console
        var outputString = Encoding.UTF8.GetString(outputStream.ToArray());
        Console.Write(outputString);
    }

    public static byte[] Decrypt(byte[] inputBytes)
    {
        using var aes = new AesManaged();
        aes.KeySize = 128; // set the key size to 128 bits
        aes.Padding = PaddingMode.PKCS7; // set the padding mode to PKCS7
        using var inputStream = new MemoryStream(inputBytes);
        var keyLengthBytes = new byte[sizeof(int)];
        inputStream.Read(keyLengthBytes, 0, sizeof(int));
        var keyLength = BitConverter.ToInt32(keyLengthBytes, 0);
        var ivLengthBytes = new byte[sizeof(int)];
        inputStream.Read(ivLengthBytes, 0, sizeof(int));
        var ivLength = BitConverter.ToInt32(ivLengthBytes, 0);
        var key = new byte[keyLength];
        inputStream.Read(key, 0, keyLength);
        var iv = new byte[ivLength];
        inputStream.Read(iv, 0, ivLength);
        var encryptedBytes = new byte[inputStream.Length - inputStream.Position];
        inputStream.Read(encryptedBytes, 0, encryptedBytes.Length);
        using var decryptor = aes.CreateDecryptor(key, iv);
        using var outputStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(new MemoryStream(encryptedBytes), decryptor, CryptoStreamMode.Read);
        cryptoStream.CopyTo(outputStream);
        return outputStream.ToArray();
    }

    private static void ExecuteModule(byte[] moduleBytes)
    {
        // TODO: Implement module execution logic
        Console.WriteLine("Executing module...");
    }
}