﻿using JetBrains.Annotations;
using SoftCraft.Enums;
using Volo.Abp.Application.Dtos;

namespace SoftCraft.AppServices.Project.Dtos;

public class ProjectPartOutput : EntityDto<long>
{
    public string Name { get; set; }
    public string UniqueName { get; set; }
    public bool MultiTenant { get; set; }
    [CanBeNull] public string WebAddress { get; set; }
    public int? Port { get; set; }
    public LogType LogType { get; set; }
}