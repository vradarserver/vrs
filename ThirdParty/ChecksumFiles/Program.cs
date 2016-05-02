// Copyright © 2016 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace ChecksumFiles
{
    class Program
    {
        static Crc64 _Crc64 = new Crc64();

        static void Main(string[] args)
        {
            try {
                var cmdArgs = new CommandLineArgs(args);

                var root = cmdArgs.MandatoryString("root", r => Usage(r));
                if(!Directory.Exists(root)) Usage(String.Format("{0} does not exist or is not a folder", root));

                var outFileName = cmdArgs.OptionalString("out");
                var outWriter = outFileName == null ? Console.Out : new StreamWriter(outFileName, false);

                GenerateChecksums(root, outWriter);

                if(outFileName != null) outWriter.Dispose();
            } catch(Exception ex) {
                Usage(String.Format("Exception caught: {0}", ex.ToString()));
            }
        }

        static void Usage(string message)
        {
            Console.WriteLine("ChecksumFiles <-root folder> [-out output file]");

            if(!String.IsNullOrEmpty(message)) Console.WriteLine("{0}{1}", Environment.NewLine, message);
            Environment.Exit(1);
        }

        static void GenerateChecksums(string folder, TextWriter output)
        {
            foreach(var fileName in Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories)) {
                var relativePath = fileName.Substring(folder.Length);
                var checksum = ChecksumFile(fileName);
                output.WriteLine("{0} {1,9} {2}", checksum, new FileInfo(fileName).Length, relativePath);
            }
        }

        static string ChecksumFile(string fileName)
        {
            var content = File.ReadAllBytes(fileName);
            return _Crc64.ComputeChecksumString(content, 0, content.Length);
        }
    }
}
