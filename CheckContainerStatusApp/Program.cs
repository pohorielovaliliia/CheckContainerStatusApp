using Docker.DotNet.Models;
using Docker.DotNet;

//TO DO: Add more statuses!!!!


internal class Program
{
    DockerClient dockerClient = new DockerClientConfiguration()
    .CreateClient();

    static int Main(string[] args)
    {
        Program program = new Program();
        DockerClient dockerClient = program.dockerClient;

        if (dockerClient == null)
            return 1;

        var containers = ListContainers(dockerClient).Result;

        PrintHeader();
        foreach (var container in containers)
            PrintContainerInfo(container.ID, container.Names[0], container.State, container.Status);

        Console.ReadKey();

        return 0;
    }

    static void PrintHeader()
    {
        Console.WriteLine(new string('-', 86));
        Console.WriteLine(
            $"{PadRight(" Container ID", 15)}|" +
            $"{PadRight(" Name", 25)}|" +
            $"{PadRight(" State", 12)}|" +
            $"{PadRight(" Status", 30)}|"
        );
        Console.WriteLine(new string('-', 86));
    }
    static void PrintContainerInfo(string id, string name, string state, string status)
    {
        string shortId = id.Substring(0, 10);

        Console.WriteLine(
            $" {PadRight(shortId, 14)}|" +
            $" {PadRight(name, 24)}|" +
            $" {PadRight(state, 11)}|" +
            $" {PadRight(status, 29)}|"
        );
    }

    static string PadRight(string text, int totalWidth)
    {
        if (text.Length > totalWidth)
            return text.Substring(0, totalWidth - 3) + "...";
        else
            return text.PadRight(totalWidth);
    }

    private static async Task<IList<ContainerListResponse>> ListContainers(DockerClient dockerClient)
    {
        IList<ContainerListResponse> containers = await dockerClient.Containers.ListContainersAsync(
            new ContainersListParameters()
            {
                Limit = 40,
                Filters = new Dictionary<string, IDictionary<string, bool>>()
                {
                    {
                        "status", new Dictionary<string, bool>
                        {
                            { "running", true } //TO DO: Add more statuses!!!!
                        }
                    }
                }
            });
        return containers;
        //- `health`= (`starting`|`healthy`|`unhealthy`|`none`)
        //- `status =`(`created`|`restarting`|`running`|`removing`|`paused`|`exited`|`dead`)
    }

    private async Task<bool> IsRunning(string id, DockerClient dockerClient)
    {

        IDictionary<string, bool> idFilter = new Dictionary<string, bool>() { { id, true } };
        Dictionary<string, IDictionary<string, bool>> filters = new Dictionary<string, IDictionary<string, bool>>()
    {
        { "id", idFilter }
    };
        ContainersListParameters parameters = new ContainersListParameters()
        {
            All = true,
            Filters = filters
        };
        IList<ContainerListResponse> containers = await dockerClient.Containers.ListContainersAsync(parameters);

        // If it doesn't exist it's not running.
        if (containers.Count < 1)
            return false;

        // todo: could we ever have >1 container matching these filters?

        ContainerListResponse response = containers[0];
        return response.State == "running";
    }
}
