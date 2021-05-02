using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Monorepo.Core
{
    public static class SemanticVersionExtensions
    {
        /// <summary>
        /// Return the version incremented by the release type
        /// </summary>
        /// <remarks>
        /// <para>
        /// Preminor will bump the version up to the next minor release, and immediately down to pre-release.
        /// Premajor and Prepatch work the same way.
        /// </para>
        /// <para>
        /// Direct port of node-semver inc(v, release): https://github.com/npm/node-semver/blob/e79ac3a450e8bb504e78b8159e3efc70895699b8/classes/semver.js#L178.
        /// See also https://github.com/npm/node-semver#functions.
        /// </para>
        /// </remarks>
        public static SemanticVersion Increment(this SemanticVersion version, Release release, string prereleaseIdentifier = "")
        {
            var major = version.Major;
            var minor = version.Minor;
            var patch = version.Patch;
            IList<string> releaseLabels = new List<string>(version.ReleaseLabels);

            void IncrementPatch()
            {
                if (!releaseLabels.Any())
                {
                    patch++;
                }
                releaseLabels.Clear();
            }

            var applyPre = false;
            switch (release)
            {
                case Release.Premajor:
                    releaseLabels.Clear();
                    patch = 0;
                    minor = 0;
                    major++;
                    applyPre = true;
                    break;

                case Release.Preminor:
                    releaseLabels.Clear();
                    patch = 0;
                    minor++;
                    applyPre = true;
                    break;

                case Release.Prepatch:
                    // If this is already a prerelease, it will bump to the next version.
                    // Drop any prereleases that might already exist, since they are not relevant at this point.
                    releaseLabels.Clear();
                    IncrementPatch();
                    applyPre = true;
                    break;

                case Release.Prerelease:
                    // If the input is a non-prerelease version, this acts the same as prepatch.
                    if (!releaseLabels.Any())
                    {
                        IncrementPatch();
                    }
                    applyPre = true;
                    break;

                case Release.Major:
                    // If this is a pre-major version, bump up to the same major version. Otherwise increment major.
                    // 1.0.0-5 bumps to 1.0.0
                    // 1.1.0 bumps to 2.0.0
                    if (minor != 0 || patch != 0 || !releaseLabels.Any())
                    {
                        major++;
                    }
                    minor = 0;
                    patch = 0;
                    releaseLabels.Clear();
                    break;

                case Release.Minor:
                    // If this is a pre-minor version, bump up to the same minor version. Otherwise increment minor.
                    // 1.2.0-5 bumps to 1.2.0
                    // 1.2.1 bumps to 1.3.0
                    if (patch != 0 || !releaseLabels.Any())
                    {
                        minor++;
                    }
                    patch = 0;
                    releaseLabels.Clear();
                    break;

                case Release.Patch:
                    // If this is not a pre-release version, it will increment the patch.
                    // If it is a pre-release it will bump up to the same patch version.
                    // 1.2.0-5 patches to 1.2.0
                    // 1.2.0 patches to 1.2.1
                    IncrementPatch();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(release), release, message: null);
            }

            if (applyPre)
            {
                if (!releaseLabels.Any())
                {
                    releaseLabels.Add("0");
                }
                else
                {
                    var i = releaseLabels.Count;
                    while (--i >= 0)
                    {
                        if (int.TryParse(releaseLabels[i], out var number))
                        {
                            releaseLabels[i] = $"{number + 1}";
                            i = -2;
                        }
                    }

                    if (i == -1)
                    {
                        // Didn't increment anything
                        releaseLabels.Add("0");
                    }
                }

                if (!string.IsNullOrWhiteSpace(prereleaseIdentifier))
                {
                    // 1.2.0-beta.1 bumps to 1.2.0-beta.2,
                    // 1.2.0-beta.fooblz or 1.2.0-beta bumps to 1.2.0-beta.0
                    if (releaseLabels.Any() && releaseLabels.First() == prereleaseIdentifier)
                    {
                        if (releaseLabels.Count < 2 || !int.TryParse(releaseLabels[1], out _))
                        {
                            releaseLabels = new[] { prereleaseIdentifier, "0" };
                        }
                    }
                    else
                    {
                        releaseLabels = new[] { prereleaseIdentifier, "0" };
                    }
                }
            }

            return new SemanticVersion(major, minor, patch, releaseLabels, metadata: null);
        }
    }
}
