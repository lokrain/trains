#nullable enable
using System;
using System.Collections.Generic;

namespace OpenTTD.Infra.Determinism
{
    /// <summary>
    /// Minimal deterministic replay/hash harness scaffold.
    /// Stores hash sequences for baseline and candidate runs and compares them tick-by-tick.
    /// </summary>
    public sealed class ReplayDeterminismHarness
    {
        private readonly List<HashSample> _baseline = new List<HashSample>(4096);
        private readonly List<HashSample> _candidate = new List<HashSample>(4096);

        /// <summary>
        /// Clears all captured samples.
        /// </summary>
        public void Reset()
        {
            _baseline.Clear();
            _candidate.Clear();
        }

        /// <summary>
        /// Appends one sample to baseline sequence.
        /// </summary>
        public void AddBaseline(HashSample sample)
        {
            _baseline.Add(sample);
        }

        /// <summary>
        /// Appends one sample to candidate sequence.
        /// </summary>
        public void AddCandidate(HashSample sample)
        {
            _candidate.Add(sample);
        }

        /// <summary>
        /// Compares baseline and candidate sequences.
        /// </summary>
        /// <param name="firstDivergentTick">First divergent tick if mismatch detected.</param>
        /// <returns>True when both sequences are identical in length and content.</returns>
        public bool TryValidate(out ulong firstDivergentTick)
        {
            firstDivergentTick = 0;

            if (_baseline.Count != _candidate.Count)
            {
                firstDivergentTick = _baseline.Count < _candidate.Count
                    ? (_baseline.Count == 0 ? 0UL : _baseline[_baseline.Count - 1].Tick)
                    : (_candidate.Count == 0 ? 0UL : _candidate[_candidate.Count - 1].Tick);
                return false;
            }

            for (int i = 0; i < _baseline.Count; i++)
            {
                if (!_baseline[i].Equals(_candidate[i]))
                {
                    firstDivergentTick = _baseline[i].Tick;
                    return false;
                }
            }

            return true;
        }
    }
}
