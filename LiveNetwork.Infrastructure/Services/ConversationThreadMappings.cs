using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveNetwork.Infrastructure.Services
{
    using System.Security.Cryptography;
    using System.Text;
    using LiveNetwork.Domain;
    using System.Globalization;
    using InviteStatus = LiveNetwork.Domain.InviteStatus;
    using Invite = LiveNetwork.Domain.Invite;
    using Message = LiveNetwork.Domain.Message;
    using MessageStatus = LiveNetwork.Domain.MessageStatus;
    public static class ConversationThreadMappings
    {
        /// <summary>
        /// Convert a set of external ConversationThreads (Models) into domain Profiles (LiveNetwork.Domain).
        /// Deterministic IDs are generated from stable fields (e.g., URLs, names, payloads).
        /// </summary>
        public static List<Profile> ToDomainProfiles(this IEnumerable<ConversationThread> threads)
        {
            if (threads is null) throw new ArgumentNullException(nameof(threads));

            var result = new List<Profile>();
            foreach (var t in threads)
            {
                if (t?.TargetProfile is null) continue;

                var mp = t.TargetProfile;

                // ---- Profile (deterministic Id from Url or FullName) ----
                var profileIdSeed = mp.Url?.ToString() ?? mp.FullName ?? Guid.NewGuid().ToString("N");
                var profileId = Guid.NewGuid().ToString();

                var dp = new Profile(profileId)
                {
                    FullName = mp.FullName ?? string.Empty,
                    Headline = mp.Headline ?? string.Empty,
                    Location = mp.Location ?? string.Empty,
                    CurrentCompany = mp.CurrentCompany ?? string.Empty,
                    ProfileImageUrl = mp.ProfileImageUrl ?? string.Empty,
                    BackgroundImageUrl = mp.BackgroundImageUrl ?? string.Empty,
                    ConnectionDegree = mp.ConnectionDegree ?? string.Empty,
                    Connections = mp.Connections ?? string.Empty,
                    Followers = mp.Followers ?? string.Empty,
                    AboutText = mp.AboutText ?? string.Empty,
                    Url = mp.Url ?? new Uri("about:blank"),
                };

                // ---- Experiences ----
                if (mp.Experiences is not null)
                {
                    var seenRoleIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var xe in mp.Experiences)
                    {
                        var expId = Guid.NewGuid().ToString();
                        var de = new Experience(expId)
                        {
                            Company = xe.Company ?? string.Empty,
                            CompanyUrl = xe.CompanyUrl ?? string.Empty,
                            CompanyLogoUrl = xe.CompanyLogoUrl ?? string.Empty,
                            CompanyLogoAlt = xe.CompanyLogoAlt ?? string.Empty,
                            EmploymentSummary = xe.EmploymentSummary ?? string.Empty,
                            Location = xe.Location ?? string.Empty,
                            Roles = []
                        };

                        if (xe.Roles is not null)
                        {
                            foreach (var xr in xe.Roles)
                            {
                                // ID determinista basado en la experiencia y los campos del rol
                                var baseId = MakeId($"ExperienceRole|{de.Id}|{xr.Title}|{xr.DateRange}|{xr.WorkArrangement}");
                                // Si ya existe en esta ejecución/perfil, crea uno nuevo para evitar colisión de tracking
                                var roleId = seenRoleIds.Add(baseId) ? baseId : Guid.NewGuid().ToString();

                                var dr = new ExperienceRole(roleId)
                                {
                                    Title = xr.Title ?? string.Empty,
                                    DateRange = xr.DateRange ?? string.Empty,
                                    WorkArrangement = xr.WorkArrangement ?? string.Empty,
                                    Description = xr.Description ?? string.Empty,
                                    ContextualSkills = xr.ContextualSkills ?? string.Empty
                                };
                                de.Roles.Add(dr);
                            }
                        }

                        dp.AddExperience(de);
                    }
                }

                // ---- Educations ----
                if (mp.Educations is not null)
                {
                    foreach (var xe in mp.Educations)
                    {
                        var eduId = Guid.NewGuid().ToString();
                        var de = new Education(eduId)
                        {
                            School = xe.School ?? string.Empty,
                            SchoolUrl = xe.SchoolUrl ?? string.Empty,
                            LogoUrl = xe.LogoUrl ?? string.Empty,
                            LogoAlt = xe.LogoAlt ?? string.Empty,
                            Degree = xe.Degree ?? string.Empty,
                            Field = xe.Field ?? string.Empty,
                            DateRange = xe.DateRange ?? string.Empty,
                            Description = xe.Description ?? string.Empty
                        };
                        dp.AddEducation(de);
                    }
                }

                // ---- Communications (Invite / Message) ----
                if (t.Communications is not null)
                {
                    foreach (var xc in t.Communications)
                    {
                        if (xc is null) continue;

                        var commIdSeed = $"{xc.TypeName}|{xc.CreateDate.ToUniversalTime():O}|{xc.Content}|{xc.Experiment}";
                        var commId = Guid.NewGuid().ToString();

                        //Discriminate by TypeName, map status safely
                        if (string.Equals(xc.TypeName, nameof(Invite), StringComparison.Ordinal))
                        {
                            var status = ParseEnumSafe<ConnectionStatus>(xc.Status, ConnectionStatus.Draft);
                            var invite = new ConnectionInvite(commId, xc.Content ?? string.Empty, xc.Experiment ?? string.Empty, status);
                            dp.AddInvite(invite);
                        }
                        else if (string.Equals(xc.TypeName, nameof(Message), StringComparison.Ordinal))
                        {
                            var status = ParseEnumSafe<InteractionStatus>(xc.Status, InteractionStatus.Draft);
                            var message = new MessageInteraction(commId, xc.Content ?? string.Empty, xc.Experiment ?? string.Empty, status);
                            dp.AddMessage(message);
                        }
                        else
                        {
                           // Unknown type: ignore or log; here we ignore.
                            continue;
                        }
                    }
                }

                result.Add(dp);
            }

            return result;
        }

        // ---------- helpers ----------

        private static TEnum ParseEnumSafe<TEnum>(string? value, TEnum @default) where TEnum : struct, Enum
            => Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed) ? parsed : @default;

        private static string MakeId(string seed)
        {
            if (string.IsNullOrWhiteSpace(seed)) seed = Guid.NewGuid().ToString("N");
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(seed));
            // 16 bytes -> 32 hex chars (compact but stable)
            return Convert.ToHexString(bytes, 0, 16);
        }
    }

}
