using System.Data;
using Dapper;

namespace SimasTurbo.Config
{
    public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
    {
        public override void SetValue(IDbDataParameter parameter, DateOnly date)
        {
            parameter.Value = date.ToDateTime(new TimeOnly(0, 0));
        }

        public override DateOnly Parse(object value)
        {
            if (value is DateOnly dateOnly) 
                return dateOnly;

   
            if (value is DateTime dateTime) 
                return DateOnly.FromDateTime(dateTime);

            if (DateOnly.TryParse(value.ToString(), out var parsedDate))
                return parsedDate;

    
    throw new InvalidCastException($"Não foi possível converter o valor '{value}' do tipo {value.GetType()} para DateOnly.");
        }
    }
}