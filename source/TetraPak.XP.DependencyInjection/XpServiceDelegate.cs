using System;

namespace TetraPak.XP.DependencyInjection;

public delegate void XpServiceDelegate(IServiceProvider provider, Type requestedType, ref object? service);