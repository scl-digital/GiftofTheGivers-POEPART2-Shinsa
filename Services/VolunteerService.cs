using DisasterAlleviationFoundation.Models;

namespace DisasterAlleviationFoundation.Services
{
    public class VolunteerService : IVolunteerService
    {
        private readonly List<VolunteerProfile> _volunteerProfiles = new();
        private readonly List<VolunteerTask> _tasks = new();
        private readonly List<VolunteerTaskAssignment> _assignments = new();
        private readonly List<VolunteerAvailability> _availability = new();
        private readonly List<VolunteerCommunication> _communications = new();
        private readonly IAuthenticationService _authService;

        public VolunteerService(IAuthenticationService authService)
        {
            _authService = authService;
            InitializeSampleData();
        }

        // Volunteer Profile Management
        public async Task<VolunteerProfile?> GetVolunteerProfileAsync(int userId)
        {
            return _volunteerProfiles.FirstOrDefault(v => v.UserId == userId);
        }

        public async Task<VolunteerProfile> CreateVolunteerProfileAsync(int userId, VolunteerRegistrationViewModel model)
        {
            var user = await _authService.GetUserByIdAsync(userId);
            if (user == null) throw new ArgumentException("User not found");

            var profile = new VolunteerProfile
            {
                Id = _volunteerProfiles.Count > 0 ? _volunteerProfiles.Max(v => v.Id) + 1 : 1,
                UserId = userId,
                User = user,
                PhoneNumber = model.PhoneNumber,
                EmergencyContact = model.EmergencyContact,
                EmergencyContactPhone = model.EmergencyContactPhone,
                Skills = model.Skills,
                Availability = model.Availability,
                HasTransportation = model.HasTransportation,
                HasMedicalTraining = model.HasMedicalTraining,
                Languages = model.Languages,
                PreviousExperience = model.PreviousExperience,
                RegistrationDate = DateTime.UtcNow,
                Status = VolunteerStatus.Active,
                BackgroundCheckStatus = BackgroundCheckStatus.Pending
            };

            _volunteerProfiles.Add(profile);
            return profile;
        }

        public async Task<VolunteerProfile> UpdateVolunteerProfileAsync(VolunteerProfile profile)
        {
            var existingProfile = _volunteerProfiles.FirstOrDefault(v => v.Id == profile.Id);
            if (existingProfile != null)
            {
                var index = _volunteerProfiles.IndexOf(existingProfile);
                _volunteerProfiles[index] = profile;
            }
            return profile;
        }

        public async Task<List<VolunteerProfile>> GetAllVolunteersAsync()
        {
            return _volunteerProfiles.ToList();
        }

        public async Task<List<VolunteerProfile>> SearchVolunteersAsync(string searchTerm, string skills, VolunteerStatus? status)
        {
            var query = _volunteerProfiles.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(v => v.User.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                        v.User.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                        v.User.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(skills))
            {
                query = query.Where(v => v.Skills.Contains(skills, StringComparison.OrdinalIgnoreCase));
            }

            if (status.HasValue)
            {
                query = query.Where(v => v.Status == status.Value);
            }

            return query.ToList();
        }

        // Task Management
        public async Task<List<VolunteerTask>> GetAvailableTasksAsync(TaskSearchViewModel? searchModel = null)
        {
            var query = _tasks.Where(t => t.Status == Models.TaskStatus.Open).AsQueryable();

            if (searchModel != null)
            {
                if (!string.IsNullOrEmpty(searchModel.SearchTerm))
                {
                    query = query.Where(t => t.Title.Contains(searchModel.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                                            t.Description.Contains(searchModel.SearchTerm, StringComparison.OrdinalIgnoreCase));
                }

                if (searchModel.Category.HasValue)
                {
                    query = query.Where(t => t.Category == searchModel.Category.Value);
                }

                if (searchModel.Priority.HasValue)
                {
                    query = query.Where(t => t.Priority == searchModel.Priority.Value);
                }

                if (!string.IsNullOrEmpty(searchModel.Location))
                {
                    query = query.Where(t => t.Location.Contains(searchModel.Location, StringComparison.OrdinalIgnoreCase));
                }

                if (searchModel.StartDate.HasValue)
                {
                    query = query.Where(t => t.StartDate >= searchModel.StartDate.Value);
                }

                if (searchModel.EndDate.HasValue)
                {
                    query = query.Where(t => t.EndDate <= searchModel.EndDate.Value);
                }
            }

            return query.OrderBy(t => t.StartDate).ToList();
        }

        public async Task<VolunteerTask?> GetTaskAsync(int taskId)
        {
            return _tasks.FirstOrDefault(t => t.Id == taskId);
        }

        public async Task<VolunteerTask> CreateTaskAsync(VolunteerTask task)
        {
            task.Id = _tasks.Count > 0 ? _tasks.Max(t => t.Id) + 1 : 1;
            task.CreatedDate = DateTime.UtcNow;
            _tasks.Add(task);
            return task;
        }

        public async Task<VolunteerTask> UpdateTaskAsync(VolunteerTask task)
        {
            var existingTask = _tasks.FirstOrDefault(t => t.Id == task.Id);
            if (existingTask != null)
            {
                var index = _tasks.IndexOf(existingTask);
                _tasks[index] = task;
            }
            return task;
        }

        public async Task<bool> DeleteTaskAsync(int taskId)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == taskId);
            if (task != null)
            {
                _tasks.Remove(task);
                return true;
            }
            return false;
        }

        public async Task<List<VolunteerTask>> GetTasksByStatusAsync(Models.TaskStatus status)
        {
            return _tasks.Where(t => t.Status == status).ToList();
        }

        public async Task<List<VolunteerTask>> GetTasksByCategoryAsync(TaskCategory category)
        {
            return _tasks.Where(t => t.Category == category).ToList();
        }

        // Task Assignment
        public async Task<VolunteerTaskAssignment?> AssignTaskAsync(int taskId, int volunteerProfileId)
        {
            var task = await GetTaskAsync(taskId);
            var volunteer = _volunteerProfiles.FirstOrDefault(v => v.Id == volunteerProfileId);

            if (task == null || volunteer == null) return null;

            // Check if volunteer is already assigned to this task
            if (_assignments.Any(a => a.TaskId == taskId && a.VolunteerProfileId == volunteerProfileId))
                return null;

            // Check if task has reached maximum volunteers
            var currentAssignments = _assignments.Count(a => a.TaskId == taskId && 
                a.Status != AssignmentStatus.Declined && a.Status != AssignmentStatus.Cancelled);
            if (currentAssignments >= task.MaxVolunteers) return null;

            var assignment = new VolunteerTaskAssignment
            {
                Id = _assignments.Count > 0 ? _assignments.Max(a => a.Id) + 1 : 1,
                TaskId = taskId,
                Task = task,
                VolunteerProfileId = volunteerProfileId,
                VolunteerProfile = volunteer,
                AssignedDate = DateTime.UtcNow,
                Status = AssignmentStatus.Assigned
            };

            _assignments.Add(assignment);
            return assignment;
        }

        public async Task<VolunteerTaskAssignment?> GetTaskAssignmentAsync(int assignmentId)
        {
            return _assignments.FirstOrDefault(a => a.Id == assignmentId);
        }

        public async Task<VolunteerTaskAssignment> UpdateTaskAssignmentAsync(VolunteerTaskAssignment assignment)
        {
            var existingAssignment = _assignments.FirstOrDefault(a => a.Id == assignment.Id);
            if (existingAssignment != null)
            {
                var index = _assignments.IndexOf(existingAssignment);
                _assignments[index] = assignment;
            }
            return assignment;
        }

        public async Task<List<VolunteerTaskAssignment>> GetVolunteerAssignmentsAsync(int volunteerProfileId)
        {
            return _assignments.Where(a => a.VolunteerProfileId == volunteerProfileId).ToList();
        }

        public async Task<List<VolunteerTaskAssignment>> GetTaskAssignmentsAsync(int taskId)
        {
            return _assignments.Where(a => a.TaskId == taskId).ToList();
        }

        public async Task<bool> AcceptTaskAssignmentAsync(int assignmentId)
        {
            var assignment = await GetTaskAssignmentAsync(assignmentId);
            if (assignment != null && assignment.Status == AssignmentStatus.Assigned)
            {
                assignment.Status = AssignmentStatus.Accepted;
                await UpdateTaskAssignmentAsync(assignment);
                return true;
            }
            return false;
        }

        public async Task<bool> DeclineTaskAssignmentAsync(int assignmentId, string reason)
        {
            var assignment = await GetTaskAssignmentAsync(assignmentId);
            if (assignment != null && assignment.Status == AssignmentStatus.Assigned)
            {
                assignment.Status = AssignmentStatus.Declined;
                assignment.Notes = reason;
                await UpdateTaskAssignmentAsync(assignment);
                return true;
            }
            return false;
        }

        public async Task<bool> CompleteTaskAssignmentAsync(int assignmentId, decimal hoursWorked, string notes)
        {
            var assignment = await GetTaskAssignmentAsync(assignmentId);
            if (assignment != null && (assignment.Status == AssignmentStatus.Accepted || assignment.Status == AssignmentStatus.InProgress))
            {
                assignment.Status = AssignmentStatus.Completed;
                assignment.HoursWorked = hoursWorked;
                assignment.Notes = notes;
                assignment.CompletionDate = DateTime.UtcNow;
                await UpdateTaskAssignmentAsync(assignment);
                return true;
            }
            return false;
        }

        // Scheduling
        public async Task<List<VolunteerAvailability>> GetVolunteerAvailabilityAsync(int volunteerProfileId)
        {
            return _availability.Where(a => a.VolunteerProfileId == volunteerProfileId).ToList();
        }

        public async Task<VolunteerAvailability> SetVolunteerAvailabilityAsync(VolunteerAvailability availability)
        {
            var existing = _availability.FirstOrDefault(a => a.VolunteerProfileId == availability.VolunteerProfileId && 
                a.DayOfWeek == availability.DayOfWeek);

            if (existing != null)
            {
                var index = _availability.IndexOf(existing);
                availability.Id = existing.Id;
                _availability[index] = availability;
            }
            else
            {
                availability.Id = _availability.Count > 0 ? _availability.Max(a => a.Id) + 1 : 1;
                _availability.Add(availability);
            }

            return availability;
        }

        public async Task<bool> DeleteVolunteerAvailabilityAsync(int availabilityId)
        {
            var availability = _availability.FirstOrDefault(a => a.Id == availabilityId);
            if (availability != null)
            {
                _availability.Remove(availability);
                return true;
            }
            return false;
        }

        public async Task<List<VolunteerProfile>> GetAvailableVolunteersAsync(DateTime startDate, DateTime endDate)
        {
            // Simplified logic - in real implementation would check detailed availability
            return _volunteerProfiles.Where(v => v.Status == VolunteerStatus.Active).ToList();
        }

        // Communication
        public async Task<VolunteerCommunication> SendCommunicationAsync(VolunteerCommunication communication)
        {
            communication.Id = _communications.Count > 0 ? _communications.Max(c => c.Id) + 1 : 1;
            communication.SentDate = DateTime.UtcNow;
            _communications.Add(communication);
            return communication;
        }

        public async Task<List<VolunteerCommunication>> GetVolunteerCommunicationsAsync(int volunteerProfileId)
        {
            return _communications.Where(c => c.VolunteerProfileId == volunteerProfileId)
                .OrderByDescending(c => c.SentDate).ToList();
        }

        public async Task<List<VolunteerCommunication>> GetTaskCommunicationsAsync(int taskId)
        {
            return _communications.Where(c => c.TaskId == taskId)
                .OrderByDescending(c => c.SentDate).ToList();
        }

        public async Task<bool> MarkCommunicationAsReadAsync(int communicationId)
        {
            var communication = _communications.FirstOrDefault(c => c.Id == communicationId);
            if (communication != null)
            {
                communication.IsRead = true;
                return true;
            }
            return false;
        }

        // Statistics and Reporting
        public async Task<VolunteerStats> GetVolunteerStatsAsync(int volunteerProfileId)
        {
            var assignments = _assignments.Where(a => a.VolunteerProfileId == volunteerProfileId).ToList();
            var completedAssignments = assignments.Where(a => a.Status == AssignmentStatus.Completed).ToList();

            return new VolunteerStats
            {
                TotalHoursWorked = (int)completedAssignments.Sum(a => a.HoursWorked),
                TasksCompleted = completedAssignments.Count,
                ActiveTasks = assignments.Count(a => a.Status == AssignmentStatus.Accepted || a.Status == AssignmentStatus.InProgress),
                AverageRating = completedAssignments.Where(a => a.Rating.HasValue).Average(a => a.Rating ?? 0),
                DaysActive = (DateTime.UtcNow - _volunteerProfiles.First(v => v.Id == volunteerProfileId).RegistrationDate).Days
            };
        }

        public async Task<Dictionary<string, object>> GetVolunteerSystemStatsAsync()
        {
            return new Dictionary<string, object>
            {
                ["TotalVolunteers"] = _volunteerProfiles.Count,
                ["ActiveVolunteers"] = _volunteerProfiles.Count(v => v.Status == VolunteerStatus.Active),
                ["TotalTasks"] = _tasks.Count,
                ["OpenTasks"] = _tasks.Count(t => t.Status == Models.TaskStatus.Open),
                ["CompletedTasks"] = _tasks.Count(t => t.Status == Models.TaskStatus.Completed),
                ["TotalAssignments"] = _assignments.Count,
                ["ActiveAssignments"] = _assignments.Count(a => a.Status == AssignmentStatus.Accepted || a.Status == AssignmentStatus.InProgress),
                ["CompletedAssignments"] = _assignments.Count(a => a.Status == AssignmentStatus.Completed),
                ["TotalHoursWorked"] = _assignments.Where(a => a.Status == AssignmentStatus.Completed).Sum(a => a.HoursWorked)
            };
        }

        public async Task<List<VolunteerTaskAssignment>> GetVolunteerHistoryAsync(int volunteerProfileId)
        {
            return _assignments.Where(a => a.VolunteerProfileId == volunteerProfileId)
                .OrderByDescending(a => a.AssignedDate).ToList();
        }

        // Dashboard
        public async Task<VolunteerDashboardViewModel> GetVolunteerDashboardAsync(int userId)
        {
            var profile = await GetVolunteerProfileAsync(userId);
            if (profile == null) throw new ArgumentException("Volunteer profile not found");

            var assignments = await GetVolunteerAssignmentsAsync(profile.Id);
            var stats = await GetVolunteerStatsAsync(profile.Id);
            var communications = await GetVolunteerCommunicationsAsync(profile.Id);
            var availableTasks = await GetRecommendedTasksAsync(profile.Id);

            return new VolunteerDashboardViewModel
            {
                Profile = profile,
                ActiveAssignments = assignments.Where(a => a.Status == AssignmentStatus.Accepted || a.Status == AssignmentStatus.InProgress).ToList(),
                CompletedAssignments = assignments.Where(a => a.Status == AssignmentStatus.Completed).Take(5).ToList(),
                RecentCommunications = communications.Take(5).ToList(),
                Stats = stats,
                AvailableTasks = availableTasks
            };
        }

        public async Task<List<VolunteerTask>> GetRecommendedTasksAsync(int volunteerProfileId)
        {
            var profile = _volunteerProfiles.FirstOrDefault(v => v.Id == volunteerProfileId);
            if (profile == null) return new List<VolunteerTask>();

            var availableTasks = await GetAvailableTasksAsync();
            
            // Simple recommendation based on skills and previous tasks
            var skills = profile.Skills.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim().ToLower()).ToList();

            return availableTasks.Where(t => 
                skills.Any(skill => t.RequiredSkills.ToLower().Contains(skill)) ||
                (t.Category == TaskCategory.EmergencyResponse && profile.HasMedicalTraining)
            ).Take(5).ToList();
        }

        // Validation
        public async Task<bool> CanVolunteerTakeTaskAsync(int volunteerProfileId, int taskId)
        {
            var volunteer = _volunteerProfiles.FirstOrDefault(v => v.Id == volunteerProfileId);
            var task = _tasks.FirstOrDefault(t => t.Id == taskId);

            if (volunteer == null || task == null) return false;
            if (volunteer.Status != VolunteerStatus.Active) return false;
            if (task.Status != Models.TaskStatus.Open) return false;

            // Check if already assigned
            if (_assignments.Any(a => a.TaskId == taskId && a.VolunteerProfileId == volunteerProfileId &&
                a.Status != AssignmentStatus.Declined && a.Status != AssignmentStatus.Cancelled))
                return false;

            // Check if task is full
            var currentAssignments = _assignments.Count(a => a.TaskId == taskId && 
                a.Status != AssignmentStatus.Declined && a.Status != AssignmentStatus.Cancelled);
            if (currentAssignments >= task.MaxVolunteers) return false;

            return true;
        }

        public async Task<bool> IsVolunteerAvailableAsync(int volunteerProfileId, DateTime startDate, DateTime endDate)
        {
            // Simplified availability check
            var volunteer = _volunteerProfiles.FirstOrDefault(v => v.Id == volunteerProfileId);
            return volunteer?.Status == VolunteerStatus.Active;
        }

        private void InitializeSampleData()
        {
            // Sample tasks
            var sampleTasks = new List<VolunteerTask>
            {
                new VolunteerTask
                {
                    Id = 1,
                    Title = "Emergency Food Distribution",
                    Description = "Help distribute emergency food supplies to flood victims in downtown area",
                    Category = TaskCategory.FoodDistribution,
                    Priority = TaskPriority.High,
                    RequiredSkills = "Physical fitness, Communication skills",
                    Location = "Downtown Community Center",
                    StartDate = DateTime.Now.AddDays(1),
                    EndDate = DateTime.Now.AddDays(1).AddHours(6),
                    EstimatedHours = 6,
                    MaxVolunteers = 10,
                    Status = Models.TaskStatus.Open,
                    CreatedDate = DateTime.Now.AddDays(-2)
                },
                new VolunteerTask
                {
                    Id = 2,
                    Title = "Medical Support Station",
                    Description = "Assist medical professionals at emergency response station",
                    Category = TaskCategory.MedicalSupport,
                    Priority = TaskPriority.Critical,
                    RequiredSkills = "Medical training, First aid certification",
                    Location = "Emergency Response Center",
                    StartDate = DateTime.Now.AddHours(2),
                    EndDate = DateTime.Now.AddHours(14),
                    EstimatedHours = 12,
                    MaxVolunteers = 5,
                    Status = Models.TaskStatus.Open,
                    CreatedDate = DateTime.Now.AddDays(-1)
                },
                new VolunteerTask
                {
                    Id = 3,
                    Title = "Shelter Setup and Management",
                    Description = "Help set up temporary shelters and assist displaced families",
                    Category = TaskCategory.ShelterManagement,
                    Priority = TaskPriority.High,
                    RequiredSkills = "Organization, People skills, Physical fitness",
                    Location = "City Sports Complex",
                    StartDate = DateTime.Now.AddDays(2),
                    EndDate = DateTime.Now.AddDays(5),
                    EstimatedHours = 8,
                    MaxVolunteers = 15,
                    Status = Models.TaskStatus.Open,
                    CreatedDate = DateTime.Now.AddDays(-3)
                }
            };

            _tasks.AddRange(sampleTasks);
        }
    }
}