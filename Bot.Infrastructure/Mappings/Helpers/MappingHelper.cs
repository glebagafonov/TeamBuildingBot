using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Bot.Domain.Entities;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Type;

namespace Bot.Infrastructure.Mappings.Helpers
{
    public static class MappingHelper
    {
        public static void PropertyEnum<TObj, TEnum>(this ClassMapping<TObj> mapper, Expression<Func<TObj, TEnum>> func)
            where TObj : Entity
            where TEnum : struct
        {
            mapper.Property(func, attr => attr.Type<EnumStringType<TEnum>>());
        }

        public static void PropertyString<TObj>(this ClassMapping<TObj> mapper, Expression<Func<TObj, string>> func)
            where TObj : Entity
        {
            mapper.Property(func, attr => attr.Column(c => c.SqlType("nvarchar(255)")));
        }

        public static void PropertyLongString<TObj>(this ClassMapping<TObj> mapper, Expression<Func<TObj, string>> func)
            where TObj : Entity
        {
            mapper.Property(func, attr => attr.Column(c => c.SqlType("nvarchar(4000)")));
        }

        public static void PropertyDateTime<TObj>(this ClassMapping<TObj> mapper, Expression<Func<TObj, DateTime>> func)
            where TObj : Entity
        {
            mapper.Property(func, attr => attr.Type<UtcDateTimeType>());
        }

        public static void PropertyDateTimeNullable<TObj>(this ClassMapping<TObj> mapper, Expression<Func<TObj, DateTime?>> func)
            where TObj : Entity
        {
            mapper.Property(func, attr => attr.Type<UtcDateTimeType>());
        }

        public static void PropertyEnum<TObj, TEnum>(this SubclassMapping<TObj> mapper, Expression<Func<TObj, TEnum>> func)
            where TObj : Entity
            where TEnum : struct
        {
            mapper.Property(func, attr => attr.Type<EnumStringType<TEnum>>());
        }

        public static void PropertyEnum<TObj, TEnum>(this SubclassMapping<TObj> mapper, Expression<Func<TObj, TEnum>> func, string columnName)
            where TObj : Entity
            where TEnum : struct
        {
            mapper.Property(func, attr =>
            {
                attr.Type<EnumStringType<TEnum>>();
                attr.Column(columnName);
            });
        }

        public static void PropertyEnum<TObj, TEnum>(this ComponentMapping<TObj> mapper, Expression<Func<TObj, TEnum>> func)
            where TObj : class
            where TEnum : struct
        {
            mapper.Property(func, attr => attr.Type<EnumStringType<TEnum>>());
        }

        public static void PropertyEnumList<TObj, TEnum>(this ComponentMapping<TObj> mapper, Expression<Func<TObj, ICollection<TEnum>>> func)
            where TObj : class
            where TEnum : struct
        {
            throw new NotImplementedException();
        }

        public static void FilterSoftDeleted<TEntity, TNested>(this ICollectionPropertiesMapper<TEntity, TNested> mapper)
            where TEntity : class
            where TNested : class
        {
            mapper.Where("[State] = 'Normal'");
        }

        public static void FilterSoftDeleted(this IManyToManyMapper mapper)
        {
            mapper.Where("[State] = 'Normal'");
        }
    }
}
