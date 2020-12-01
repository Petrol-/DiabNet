using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace DiabNet.TestIT
{
    enum ContainerAction
    {
        None,
        Create
    }

    public class ElasticSearchTestContainer : IProgress<JSONMessage>
    {
        public const string Url = "http://localhost:9200";
        private DockerClient _docker;

        private string _containerId;
        private readonly string _containerName = "diabnet-test";
        private const string EsDockerImage = "docker.elastic.co/elasticsearch/elasticsearch:7.10.0";
        private readonly string _testUrl = $"{Url}/_cat/health?h=st";


        public async Task Start()
        {
            _docker = new DockerClientConfiguration().CreateClient();
            var container = await FindExistingContainer();
            if (container != null)
            {
                var action = await EnsureContainerStartedOrRemove(container);
                if (action == ContainerAction.None)
                {
                    await EnsureContainerReady();
                    return;
                }
            }

            await TryPullImage();
            await CreateAndStartContainer();
            await EnsureContainerReady();
        }

        public async Task Stop()
        {
            await _docker.Containers.RemoveContainerAsync(_containerId, new ContainerRemoveParameters() {Force = true});
        }

        private async Task TryPullImage()
        {
            var images = await _docker.Images.ListImagesAsync(new ImagesListParameters {All = true});
            var image = images.FirstOrDefault(s => s.RepoTags.Contains(EsDockerImage));
            if (image == null)
                await _docker.Images.CreateImageAsync(new ImagesCreateParameters
                {
                    FromImage = EsDockerImage,
                }, null, this);
        }

        private async Task<ContainerListResponse> FindExistingContainer()
        {
            var containers = await _docker
                .Containers
                .ListContainersAsync(new ContainersListParameters
                {
                    All = true
                });
            return containers.FirstOrDefault(container => container.Names.Contains($"/{_containerName}"));
        }

        private async Task<ContainerAction> EnsureContainerStartedOrRemove(ContainerListResponse container)
        {
            if (container.State == "running")
            {
                _containerId = container.ID;
                return ContainerAction.None;
            }

            await _docker.Containers.RemoveContainerAsync(container.ID, new ContainerRemoveParameters {Force = true});
            return ContainerAction.Create;
        }

        private async Task CreateAndStartContainer()
        {
            var response = await _docker.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                Name = _containerName,
                Image = EsDockerImage,

                Env = new List<string> {"discovery.type=single-node"},
                ExposedPorts = new Dictionary<string, EmptyStruct>
                {
                    {
                        "9200", new EmptyStruct()
                    }
                },

                HostConfig = new HostConfig
                {
                    PublishAllPorts = true,
                    PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        {"9200/tcp", new List<PortBinding> {new PortBinding {HostPort = "9200"}}}
                    }
                }
            });
            _containerId = response.ID;
            await _docker.Containers.StartContainerAsync(_containerId, new ContainerStartParameters());
        }

        private async Task EnsureContainerReady()
        {
            var maxNbTest = 30;
            var client = new HttpClient();
            for (var i = 0; i < maxNbTest; i++)
            {
                try
                {
                    var response = await client.GetAsync(_testUrl);
                    if (response.IsSuccessStatusCode)
                        return;
                }
                catch
                {
                    //skip
                }

                await Task.Delay(1000);
            }

            throw new Exception("Container is not ready after 30 sec");
        }

        public void Report(JSONMessage value)
        {
            Console.WriteLine(value.Status);
        }
    }
}
