using vmPing.Configuration;

namespace vmPing.Probes
{
    public partial class Probe
    {
        // Updates statistics and status after a host replies.
        private void HandleReply(long roundTripTime)
        {
            Statistics.Received++;
            IndeterminateCount = 0;

            // If this is a new probe, record the initial 'up' state to the status history.
            if (Status == ProbeStatus.Inactive)
            {
                AddStatusHistory(ProbeStatus.Up, true);
                Status = ProbeStatus.Up;
            }

            // Check if status changed from down to up.
            if (Status == ProbeStatus.Down)
            {
                OnStatusChange(ProbeStatus.Up, "up");
            }

            UpdateLatency(roundTripTime);
        }

        private void UpdateLatency(long roundTripTime)
        {
            // Update minimum RTT.
            if (roundTripTime < MinRtt)
            {
                MinRtt = roundTripTime;
            }

            // Check latency.
            if ((ApplicationOptions.LatencyDetectionMode == LatencyMode.Fixed &&
                roundTripTime >= ApplicationOptions.HighLatencyMilliseconds) ||
                (ApplicationOptions.LatencyDetectionMode == LatencyMode.Dynamic &&
                roundTripTime >= MinRtt + ApplicationOptions.HighLatencyMilliseconds))
            {
                // Latency is high.
                if (HighLatencyCount < ApplicationOptions.HighLatencyAlertTriggerCount)
                {
                    HighLatencyCount++;
                }

                if (Status == ProbeStatus.Up)
                {
                    Status = ProbeStatus.Indeterminate;
                }

                if (Status != ProbeStatus.LatencyHigh)
                {
                    if (HighLatencyCount >= ApplicationOptions.HighLatencyAlertTriggerCount)
                    {
                        OnStatusChange(ProbeStatus.LatencyHigh, "high latency");
                    }
                }
            }
            else
            {
                // Latency is normal.
                if (HighLatencyCount > 0)
                {
                    HighLatencyCount--; 
                }

                if (Status == ProbeStatus.LatencyHigh)
                {
                    if (HighLatencyCount <= 0)
                    {
                        OnStatusChange(ProbeStatus.LatencyNormal, "normal latency");
                        Status = ProbeStatus.Up;
                    }
                }
                else
                {
                    Status = ProbeStatus.Up;
                }
            }
        }

        // Updates statistics and status after a host fails to reply.
        private void HandleNoReply()
        {
            Statistics.Lost++;
            IndeterminateCount++;

            if (Status == ProbeStatus.Up || Status == ProbeStatus.LatencyHigh)
            {
                Status = ProbeStatus.Indeterminate;
            }
            else if (Status == ProbeStatus.Inactive)
            {
                // Because this is a new probe, ignore the indeterminate count
                // and immediately mark the host as down.
                // Also, record the initial 'down' state to the status history.
                AddStatusHistory(ProbeStatus.Down, true);
                Status = ProbeStatus.Down;
            }

            // Check for status change.
            if (Status == ProbeStatus.Indeterminate &&
                IndeterminateCount >= ApplicationOptions.AlertThreshold)
            {
                Status = ProbeStatus.Down;
                OnStatusChange(ProbeStatus.Down, "down");
            }
        }
    }
}
