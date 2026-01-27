using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLogic.Services.Extension.Encrypt
{
    public interface IEncryptService
    {
        string Encrypt(string? data);
        string Decrypt(string? data);
    }
}