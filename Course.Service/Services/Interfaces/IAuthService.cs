﻿using Course.Service.Dtos.UserDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Course.Service.Services.Interfaces
{
    public interface IAuthService
    {
        string Login(UserLoginDto loginDto);
    }
}