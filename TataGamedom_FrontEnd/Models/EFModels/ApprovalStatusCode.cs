﻿using System;
using System.Collections.Generic;

namespace TataGamedom_FrontEnd.Models.EFModels;

public partial class ApprovalStatusCode
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<BoardsModeratorsApplication> BoardsModeratorsApplications { get; set; } = new List<BoardsModeratorsApplication>();
}
