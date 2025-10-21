using System;
using System.Collections.Generic;

namespace GameSpace.Models;

public partial class UserWallet
{
    public int UserId { get; set; }

    public int UserPoint { get; set; }

    public virtual User User { get; set; } = null!;
}
