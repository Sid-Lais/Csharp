using Pulumi;
using Pulumi.Aws.Ec2;
using Pulumi.Aws.S3;

class MyStack : Stack
{
    public MyStack()
    {
        // Create an S3 bucket
        var bucket = new Bucket("myBucket", new BucketArgs
        {
            Tags = 
            {
                { "Environment", "Dev" },
                { "Owner", "DevOps Team" }
            }
        });

        // Get the default VPC
        var defaultVpc = Output.Create(GetVpc.InvokeAsync(new GetVpcArgs { Default = true }));

        // Get a subnet from the default VPC
        var defaultSubnet = Output.Create(GetSubnet.InvokeAsync(new GetSubnetArgs
        {
            VpcId = defaultVpc.Apply(vpc => vpc.Id)
        }));

        // Create a security group
        var securityGroup = new SecurityGroup("webServerSecurityGroup", new SecurityGroupArgs
        {
            Ingress = 
            {
                new SecurityGroupIngressArgs
                {
                    Protocol = "tcp",
                    FromPort = 22,  // Allow SSH
                    ToPort = 22,
                    CidrBlocks = { "0.0.0.0/0" }
                },
                new SecurityGroupIngressArgs
                {
                    Protocol = "tcp",
                    FromPort = 80,  // Allow HTTP
                    ToPort = 80,
                    CidrBlocks = { "0.0.0.0/0" }
                }
            }
        });

        // Create an EC2 instance
        var instance = new Instance("myInstance", new InstanceArgs
        {
            Ami = "ami-12345678",  // Replace with a valid AMI ID
            InstanceType = "t2.micro",
            SubnetId = defaultSubnet.Apply(subnet => subnet.Id),
            VpcSecurityGroupIds = { securityGroup.Id },
            Tags = 
            {
                { "Name", "MyPulumiInstance" },
                { "Environment", "Dev" }
            }
        });

        // Export outputs
        this.BucketName = bucket.Id;
        this.InstancePublicIp = instance.PublicIp;
    }

    [Output] public Output<string> BucketName { get; set; }
    [Output] public Output<string> InstancePublicIp { get; set; }
}

class Program
{
    static Task<int> Main() => Deployment.RunAsync<MyStack>();
}
