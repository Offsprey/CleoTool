using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.IO;

namespace CleoTool
{
    internal class DataCompression
    {
        public static String zlipDecomp(String encodedStr)
        {
            //encodedStr = "vdfPbhtFHAfwtD30BeDEZW8cmhXenf0bqRLEaUiapJbiJEaKepju/uydZj2zzIwTIfmCuFSgPkEPlapYIn+rVs2VF0CQOy8ABxQEfQFmHRtsMWvsxK0Plm3J9sxnv7/fb/bZ01MOImNUQIWTBqE3XpD4peUFXlCy7CAwLReFHbZHgb8u40w+AswjLM3PJAGRnESM1kmjxbEkjB5Vl8qGadQqG6sr3/d/9aCSATXWWZp2ohQL0VlY31xeOAIasRaVwJfjn27+vk8kNH+ZuduO6nWMkAvQXso/mnNcG7lzvUdQ6r+aayfb6zgmmEqjCjh92E7a/Bi3YiK3gAu1lm86TUIZn+lkWEbJTKeJHzN+47C/quX45oEkTRASN7Of42+/e3JIqHpDI8gXtL2fEiF/eADRzqyxSGgD+KxRqdfNJUzjWaOaEEhjndypkrOQiyynZDquNx247mJe17h6njXmcb6mGlZvhhEveogLGkRkhXrEcsrwjsHqhkzAWMM7Cm9syyGwQU2fvBmEHrzuI8wc2/NNx3feddj+6jnd1zg5oaV36no3WpjHfawyBywZv27yfPqRJnmaiz1CzrM9ZDpBMB25MTYzDPpHD3RLA+ohRw86r/iEzFIsoS9aw1/t5cBbmH7ZIiKBS9wuyKslwKrwygnkLKvQECPi1/p1kvgdK0RkeXboOOa1CvZkLacyNrMGxzEUB/C3ntc9jZdVCvReaxAlmCqJj4UxT2isepK4QvZuDUJVb3969eydXLKp7DmmW5qe28TxuxhRz8guGB6fp2wXRD95VcnU6gFng5E7qyaslcZ558+7vlCDAEAWx676QTAI/X+xu/Tzbdt1zeuV7ri56ze+DV2dOqgACqvvpyDFOFVaQFYUvzvPhzXHuPRFpxWEUOCFoWlZFpqe5eWm1iElkchLobuljYSrPximfdujXdTQuqWC2bscs/SfacI4b2WTzJPCHG6e/ThJDnM9JwyQ6yPT8h3rXY/fP0el0C0oV9Xwu6NCXHFWFAxfev7h1BLYNcwT6Dv+FI99/92MnnNFd5oJ7ELORj5A+pqrpA4SaAxTiB89/2SS6du91bBC11XTV51Sw/cVv3tar4ImOM/Yvw2wCnxX3XdM0PWGMvf12hcTZ+7v";
            byte[] compressedB = System.Convert.FromBase64String(encodedStr);
            string rawCompressed = Encoding.UTF8.GetString(compressedB, 0, compressedB.Length);
            var stream = new MemoryStream();
            var ostream = new MemoryStream();
            stream.Write(compressedB, 0, compressedB.Length);
            stream.Position = 0;
            DeflateStream decompressor = new DeflateStream(stream, CompressionMode.Decompress);
            decompressor.CopyTo(ostream);
           string test =  decompressor.ToString();
            TextReader reader = new StreamReader(decompressor);
            File.WriteAllBytes("C:\\Users\\offsp\\Documents\\testzlip.txt", ostream.ToArray());
            byte[] strStream = ostream.ToArray();
            String outStr = "";
            int i = 0;
            foreach (byte b in strStream)
            {
                if (b != 0)
                {
                    outStr += (char)b;
                }
                i++;
            }
            String text = Encoding.UTF8.GetString(strStream,0, strStream.Length);
            return outStr;
        }
    }
}
