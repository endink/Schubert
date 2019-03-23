using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update;
using System;
using System.Collections.Generic;
using System.Text;

namespace Schubert.Framework
{
    public class UniqueConstraintViolationException : DbUpdateException
    {
        public UniqueConstraintViolationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public UniqueConstraintViolationException(string message, IReadOnlyList<IUpdateEntry> entries) : base(message, entries)
        {
        }

        public UniqueConstraintViolationException(string message, Exception innerException, IReadOnlyList<IUpdateEntry> entries) : base(message, innerException, entries)
        {
        }
    }
}
