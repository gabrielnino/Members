using System.Security.Cryptography;
using System.Text;
using LiveNetwork.Application.UseCases.CRUD.Profile.Query;
using LiveNetwork.Domain;
using Invite = LiveNetwork.Domain.Invite;
using Message = LiveNetwork.Domain.Message;

namespace LiveNetwork.Infrastructure.Services
{
    public static class ConversationThreadMappings
    {
        public static async Task<List<Profile>> ToDomainProfiles(this IEnumerable<ConversationThread> threads, IProfileRead profileRead)
        {
            if (threads is null) throw new ArgumentNullException(nameof(threads));

            // Diccionarios para validar duplicados
            var profilesById = new Dictionary<string, Profile>();
            var experiencesById = new Dictionary<string, Experience>();
            var rolesById = new Dictionary<string, ExperienceRole>();
            var educationsById = new Dictionary<string, Education>();
            var commsById = new Dictionary<string, Interaction>();

            foreach (var thread  in threads)
            {
                if (thread?.TargetProfile is null)
                {
                    continue;
                }

                var sourceProfile = thread.TargetProfile;
                var profileId = await GenerateProfileId(sourceProfile, profileRead);

                if (!profilesById.TryGetValue(profileId, out var domainProfile))
                {
                    domainProfile = CreateDomainProfile(sourceProfile, profileId);
                    profilesById[profileId] = domainProfile;
                }
                ProcessExperiences(experiencesById, rolesById, sourceProfile, profileId, domainProfile);
                ProcessEducations(educationsById, sourceProfile, profileId, domainProfile);
                ProcessCommunications(commsById, thread, profileId, domainProfile);
            }

            return [.. profilesById.Values];
        }

        private static void ProcessCommunications(Dictionary<string, Interaction> commsById, ConversationThread thread, string profileId, Profile domainProfile)
        {
            if (thread.Communications is null)
            {
                return;
            }

            foreach (var xc in thread.Communications)
            {
                if (xc is null) continue;

                var commId = MakeId($"comm|{profileId}|{xc.TypeName}|{xc.CreateDate.ToUniversalTime():O}|{xc.Content}|{xc.Experiment}");
                if (commsById.ContainsKey(commId)) continue;

                if (string.Equals(xc.TypeName, nameof(Invite), StringComparison.Ordinal))
                {
                    var status = ParseEnumSafe<ConnectionStatus>(xc.Status, ConnectionStatus.Draft);
                    var invite = new ConnectionInvite(commId, xc.Content ?? string.Empty, xc.Experiment ?? string.Empty, status);
                    commsById[commId] = invite;
                    domainProfile.AddInvite(invite);
                }
                else if (string.Equals(xc.TypeName, nameof(Message), StringComparison.Ordinal))
                {
                    var status = ParseEnumSafe<InteractionStatus>(xc.Status, InteractionStatus.Draft);
                    var message = new MessageInteraction(commId, xc.Content ?? string.Empty, xc.Experiment ?? string.Empty, status);
                    commsById[commId] = message;
                    domainProfile.AddMessage(message);
                }
            }
        }

        private static void ProcessEducations(Dictionary<string, Education> educationsById, LinkedInProfile sourceProfile, string profileId, Profile domainProfile)
        {
            if (sourceProfile.Educations is not null)
            {
                foreach (var xe in sourceProfile.Educations)
                {
                    var eduId = MakeId($"education|{profileId}|{xe.School}|{xe.Degree}|{xe.Field}|{xe.DateRange}");
                    if (!educationsById.ContainsKey(eduId))
                    {
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
                        educationsById[eduId] = de;
                        domainProfile.AddEducation(de);
                    }
                }
            }
        }

        private static void ProcessExperiences(Dictionary<string, Experience> experiencesById, Dictionary<string, ExperienceRole> rolesById, LinkedInProfile sourceProfile, string profileId, Profile domainProfile)
        {
            if (sourceProfile.Experiences is not null)
            {
                foreach (var xe in sourceProfile.Experiences)
                {
                    var expId = MakeId($"experience|{profileId}|{xe.Company}|{xe.EmploymentSummary}");
                    if (!experiencesById.ContainsKey(expId))
                    {
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
                        experiencesById[expId] = de;
                        domainProfile.AddExperience(de);
                    }

                    // Roles dentro de la experiencia
                    if (xe.Roles is not null)
                    {
                        foreach (var xr in xe.Roles)
                        {
                            var roleId = MakeId($"role|{profileId}|{xe.Company}|{xr.Title}|{xr.DateRange}|{xr.WorkArrangement}");
                            if (!rolesById.ContainsKey(roleId))
                            {
                                var dr = new ExperienceRole(roleId)
                                {
                                    Title = xr.Title ?? string.Empty,
                                    DateRange = xr.DateRange ?? string.Empty,
                                    WorkArrangement = xr.WorkArrangement ?? string.Empty,
                                    Description = xr.Description ?? string.Empty,
                                    ContextualSkills = xr.ContextualSkills ?? string.Empty
                                };
                                rolesById[roleId] = dr;
                                experiencesById[expId].Roles.Add(dr);
                            }
                        }
                    }
                }
            }
        }

        private static Profile CreateDomainProfile(LinkedInProfile sourceProfile, string profileId)
        {
            return new Profile(profileId)
            {
                FullName = sourceProfile.FullName ?? string.Empty,
                Headline = sourceProfile.Headline ?? string.Empty,
                Location = sourceProfile.Location ?? string.Empty,
                CurrentCompany = sourceProfile.CurrentCompany ?? string.Empty,
                ProfileImageUrl = sourceProfile.ProfileImageUrl ?? string.Empty,
                BackgroundImageUrl = sourceProfile.BackgroundImageUrl ?? string.Empty,
                ConnectionDegree = sourceProfile.ConnectionDegree ?? string.Empty,
                Connections = sourceProfile.Connections ?? string.Empty,
                Followers = sourceProfile.Followers ?? string.Empty,
                AboutText = sourceProfile.AboutText ?? string.Empty,
                Url = sourceProfile.Url ?? new Uri("about:blank"),
            };
        }


        private static async Task<string> GenerateProfileId(LinkedInProfile sourceProfile, IProfileRead profileRead)
        {
            var id = await profileRead.GetProfilesByUrlAsync(sourceProfile.Url?.ToString(), null, 1);
            if (id is not null && id.IsSuccessful)
            {
                if (id.Data != null)
                {
                    if (id.Data.Items != null)
                    {
                        if (id.Data.Items.FirstOrDefault() !=  null)
                        {
                            return id.Data.Items.FirstOrDefault().Id;
                        }
                    }
                }
            }
            var profileKey = (sourceProfile.Url?.ToString() ?? sourceProfile.FullName ?? Guid.NewGuid().ToString())
                .Trim().ToLowerInvariant();
            return GenerateId($"profile|{profileKey}");
        }

        private static string GenerateId(string seed)
        {
            if (string.IsNullOrWhiteSpace(seed))
                return Guid.NewGuid().ToString("N");

            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(seed));
            return Convert.ToHexString(hash, 0, 16);
        }

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
