﻿using System;
using Jbet.Core.Base;

namespace Jbet.Core.AuthContext.Commands
{
    public class Register : ICommand
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }
    }
}