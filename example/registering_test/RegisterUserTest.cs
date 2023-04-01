// using Xunit;
// using scyna;
// using Registering;

// namespace Registering.Test;

// public class RegisterUserTest : TestBase
// {
//     [Theory]
//     [InlineData("a@gmail.com", "Nguyen Van A")]
//     [InlineData("b@gmail.com", "Nguyen Van B")]
//     public void TestRegisterUser_ShouldSucess(string email, string name)
//     {
//         EndpointTest.Create(Path.REGISTER_USER_URL)
//             .WithRequest(new PROTO.RegisterUserRequest
//             {
//                 Email = email,
//                 Name = name,
//                 Password = "12345678"
//             })
//             .MatchEvent(new PROTO.RegistrationCreated
//             {
//                 Email = email,
//                 Name = name
//             })
//             .ExpectSuccess()
//             .Run();
//     }

//     [Theory]
//     [InlineData("a+gmail.com", "Nguyen Van A", "12345678")]
//     [InlineData("", "Nguyen Van A", "12345678")]
//     [InlineData("a@gmail.com", "", "12345678")]
//     [InlineData("a@gmail.com", "Nguyen Van A", "")]
//     [InlineData("a@gmail.com", "Nguyen Van A", "VeryLongPasswordShouldReturnInvalid")]
//     public void TestRegisterUser_ShouldReturnRequestInvalid(string email, string name, string password)
//     {
//         EndpointTest.Create(Path.REGISTER_USER_URL)
//             .WithRequest(new PROTO.RegisterUserRequest
//             {
//                 Email = email,
//                 Name = name,
//                 Password = password
//             })
//             .ExpectError(scyna.Error.REQUEST_INVALID)
//             .Run();
//     // }


//     [Fact]
//     public void TestRegisterUser_ShouldReturnUserExists()
//     {
//         EndpointTest.Create(Path.REGISTER_USER_URL)
//             .WithRequest(new PROTO.RegisterUserRequest
//             {
//                 Email = "a@gmail.com",
//                 Name = "Nguyen Van A",
//                 Password = "12345678"
//             })
//             .ExpectSuccess()
//             .Run();

//         EndpointTest.Create(Path.REGISTER_USER_URL)
//             .WithRequest(new PROTO.RegisterUserRequest
//             {
//                 Email = "a@gmail.com",
//                 Name = "Nguyen Van A",
//                 Password = "12345678"
//             })
//             .ExpectError(Registering.Error.EMAIL_REGISTERED)
//             .Run();
//     }
// }