using System.Diagnostics;
using Microsoft.Extensions.Options;
using RentACar.Shared.Abstractions.Time;

namespace RentACar.Shared.Infrastructure.Time;

/// <summary>Gerçek zamana göre <see cref="Speed"/> katı hızla akan, hızı çalışırken değiştirilebilen saat.</summary>
internal sealed class SimulationClock : IClock
{
    public const double MaxSpeed = 3600;

    private readonly Lock _gate = new();
    private DateTimeOffset _anchorSim;
    private long _anchorReal;
    private double _speed;

    public SimulationClock(IOptions<ClockOptions> options)
    {
        _anchorSim = (options.Value.StartAt ?? DateTimeOffset.UtcNow).ToUniversalTime();
        _anchorReal = Stopwatch.GetTimestamp();
        _speed = Math.Clamp(options.Value.Speed, 0, MaxSpeed);
    }

    public DateTimeOffset UtcNow
    {
        get
        {
            lock (_gate)
            {
                return _anchorSim + Stopwatch.GetElapsedTime(_anchorReal) * _speed;
            }
        }
    }

    public double Speed
    {
        get
        {
            lock (_gate)
            {
                return _speed;
            }
        }
    }

    public void SetSpeed(double speed)
    {
        lock (_gate)
        {
            Rebase(_anchorSim + Stopwatch.GetElapsedTime(_anchorReal) * _speed);
            _speed = Math.Clamp(speed, 0, MaxSpeed);
        }
    }

    public void Reset(DateTimeOffset utcNow)
    {
        lock (_gate)
        {
            Rebase(utcNow.ToUniversalTime());
        }
    }

    private void Rebase(DateTimeOffset sim)
    {
        _anchorSim = sim;
        _anchorReal = Stopwatch.GetTimestamp();
    }
}
