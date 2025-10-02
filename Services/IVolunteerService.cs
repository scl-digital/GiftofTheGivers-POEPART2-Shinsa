using DisasterAlleviationFoundation.Models;

namespace DisasterAlleviationFoundation.Services
{
    public interface IVolunteerService
    {
        // Volunteer Profile Management
        Task<VolunteerProfile?> GetVolunteerProfileAsync(int userId);
        Task<VolunteerProfile> CreateVolunteerProfileAsync(int userId, VolunteerRegistrationViewModel model);
        Task<VolunteerProfile> UpdateVolunteerProfileAsync(VolunteerProfile profile);
        Task<List<VolunteerProfile>> GetAllVolunteersAsync();
        Task<List<VolunteerProfile>> SearchVolunteersAsync(string searchTerm, string skills, VolunteerStatus? status);
        
        // Task Management
        Task<List<VolunteerTask>> GetAvailableTasksAsync(TaskSearchViewModel? searchModel = null);
        Task<VolunteerTask?> GetTaskAsync(int taskId);
        Task<VolunteerTask> CreateTaskAsync(VolunteerTask task);
        Task<VolunteerTask> UpdateTaskAsync(VolunteerTask task);
        Task<bool> DeleteTaskAsync(int taskId);
        Task<List<VolunteerTask>> GetTasksByStatusAsync(Models.TaskStatus status);
        Task<List<VolunteerTask>> GetTasksByCategoryAsync(TaskCategory category);
        
        // Task Assignment
        Task<VolunteerTaskAssignment?> AssignTaskAsync(int taskId, int volunteerProfileId);
        Task<VolunteerTaskAssignment?> GetTaskAssignmentAsync(int assignmentId);
        Task<VolunteerTaskAssignment> UpdateTaskAssignmentAsync(VolunteerTaskAssignment assignment);
        Task<List<VolunteerTaskAssignment>> GetVolunteerAssignmentsAsync(int volunteerProfileId);
        Task<List<VolunteerTaskAssignment>> GetTaskAssignmentsAsync(int taskId);
        Task<bool> AcceptTaskAssignmentAsync(int assignmentId);
        Task<bool> DeclineTaskAssignmentAsync(int assignmentId, string reason);
        Task<bool> CompleteTaskAssignmentAsync(int assignmentId, decimal hoursWorked, string notes);
        
        // Scheduling
        Task<List<VolunteerAvailability>> GetVolunteerAvailabilityAsync(int volunteerProfileId);
        Task<VolunteerAvailability> SetVolunteerAvailabilityAsync(VolunteerAvailability availability);
        Task<bool> DeleteVolunteerAvailabilityAsync(int availabilityId);
        Task<List<VolunteerProfile>> GetAvailableVolunteersAsync(DateTime startDate, DateTime endDate);
        
        // Communication
        Task<VolunteerCommunication> SendCommunicationAsync(VolunteerCommunication communication);
        Task<List<VolunteerCommunication>> GetVolunteerCommunicationsAsync(int volunteerProfileId);
        Task<List<VolunteerCommunication>> GetTaskCommunicationsAsync(int taskId);
        Task<bool> MarkCommunicationAsReadAsync(int communicationId);
        
        // Statistics and Reporting
        Task<VolunteerStats> GetVolunteerStatsAsync(int volunteerProfileId);
        Task<Dictionary<string, object>> GetVolunteerSystemStatsAsync();
        Task<List<VolunteerTaskAssignment>> GetVolunteerHistoryAsync(int volunteerProfileId);
        
        // Dashboard
        Task<VolunteerDashboardViewModel> GetVolunteerDashboardAsync(int userId);
        Task<List<VolunteerTask>> GetRecommendedTasksAsync(int volunteerProfileId);
        
        // Validation
        Task<bool> CanVolunteerTakeTaskAsync(int volunteerProfileId, int taskId);
        Task<bool> IsVolunteerAvailableAsync(int volunteerProfileId, DateTime startDate, DateTime endDate);
    }
}