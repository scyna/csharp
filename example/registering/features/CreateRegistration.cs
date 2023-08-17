namespace ex.registering;

using scyna;
using FluentValidation;

public class CreateRegistrationHandler : Endpoint.Handler<PROTO.CreateRegistrationRequest>
{
    public override void Execute()
    {
        var validator = new RequestValidator();
        if (!validator.Validate(request).IsValid) throw scyna.Error.REQUEST_INVALID;

        Engine.DB.AssureNotExist($@"SELECT * FROM {Table.REGISTRATION} WHERE email = ?", request.Email);
        Engine.DB.AssureNotExist($@"SELECT * FROM {Table.REGISTERED} WHERE email = ?", request.Email);

        var otp = Utils.GenerateSixDigitsOtp();
        var expired = DateTimeOffset.Now.AddMinutes(5);

        Engine.DB.ExecuteUpdate($@"INSERT INTO {Table.REGISTRATION} (email,name,password,otp,expired)
            VALUES (?, ?, ?, ?, ?)", request.Email, request.Name, request.Password, otp, expired);

        context.RaiseEvent(new PROTO.RegistrationCreated
        {
            Email = request.Email,
            Name = request.Name,
            Password = request.Password,
        });

        context.RaiseEvent(new PROTO.OtpGenerated
        {
            Email = request.Email,
            Otp = otp,
        });
    }

    class RequestValidator : AbstractValidator<PROTO.CreateRegistrationRequest>
    {
        public RequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(40);
            RuleFor(x => x.Password).NotEmpty().Length(6, 20);
        }
    }
}
