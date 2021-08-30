/*
 * Limaki 
 * 
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.
 * 
 * Author: Lytico
 * Copyright (C) 2012 - 2016 Lytico
 *
 * http://www.limada.org
 * 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LinqToDB.Data;
using LinqToDB.SchemaProvider;
using LinqToDB.SqlQuery;

namespace LinqToDB.Mapping {

    public static class LinqToDbExtensions {

        public static PropertyMappingBuilder<T, P> HasIndex<T, P> (this PropertyMappingBuilder<T, P> it) {

            it.HasAttribute (new IndexAttribute ());

            return it;
        }

        public static PropertyMappingBuilder<T, P> HasIndex<T, P> (this PropertyMappingBuilder<T, P> it, string name, int order) {

            it.HasAttribute (new IndexAttribute (name, order));

            return it;
        }

        /// <summary>
        /// Creates a commandText for mapping class <typeparamref name="T"/>.
        /// Information about table name, columns names and types is taken from mapping class.
        /// </summary>
        /// <typeparam name="T">Mapping class.</typeparam>
        /// <param name="dataContext">Database connection context.</param>
        /// <param name="tableName">Optional table name to override default table name, extracted from <typeparamref name="T"/> mapping.</param>
        /// <param name="databaseName">Optional database name, to override default database name. See <see cref="LinqExtensions.DatabaseName{T}(ITable{T}, string)"/> method for support information per provider.</param>
        /// <param name="schemaName">Optional schema/owner name, to override default name. See <see cref="LinqExtensions.SchemaName{T}(ITable{T}, string)"/> method for support information per provider.</param>
        /// <param name="statementHeader">Optional replacement for <c>"CREATE TABLE table_name"</c> header. Header is a template with <c>{0}</c> parameter for table name.</param>
        /// <param name="statementFooter">Optional SQL, appended to generated create table statement.</param>
        /// <param name="defaultNullable">Defines how columns nullability flag should be generated:
        /// <para> - <see cref="DefaultNullable.Null"/> - generate only <c>NOT NULL</c> for non-nullable fields. Missing nullability information treated as <c>NULL</c> by database.</para>
        /// <para> - <see cref="DefaultNullable.NotNull"/> - generate only <c>NULL</c> for nullable fields. Missing nullability information treated as <c>NOT NULL</c> by database.</para>
        /// <para> - <see cref="DefaultNullable.None"/> - explicitly generate <c>NULL</c> and <c>NOT NULL</c> for all columns.</para>
        /// Default value: <see cref="DefaultNullable.None"/>.
        /// </param>
        /// <returns>Created sql command</returns>
        public static string CreateTableStatement<T> (this IDataContext dataContext,
            string tableName = null,
            string databaseName = null,
            string schemaName = null,
            string statementHeader = null,
            string statementFooter = null,
            DefaultNullable defaultNullable = DefaultNullable.None) {
            if (dataContext == null) throw new ArgumentNullException (nameof(dataContext));

            return Query<T> (dataContext,
                tableName, databaseName, schemaName, statementHeader, statementFooter, defaultNullable);
        }

        public static string Query<T> (IDataContext dataContext,
            string tableName, string databaseName, string schemaName,
            string statementHeader, string statementFooter,
            DefaultNullable defaultNullable) {
            var sqlTable = new SqlTable<T> (dataContext.MappingSchema);
            var createTable = new SqlCreateTableStatement ();

            if (tableName != null) sqlTable.PhysicalName = tableName;
            if (databaseName != null) sqlTable.Database = databaseName;
            if (schemaName != null) sqlTable.Schema = schemaName;

            createTable.Table = sqlTable;
            createTable.StatementHeader = statementHeader;
            createTable.StatementFooter = statementFooter;
            createTable.DefaultNullable = defaultNullable;

            return createTable.SqlText;

        }

        public static DatabaseSchema DatabaseSchema (this DataConnection it)
            => it.DataProvider.GetSchemaProvider ().GetSchema (it, new GetSchemaOptions {GetTables = true, GetProcedures = false,});

        public static IList<TableSchema> TableSchemas (this DataConnection it)
            => it.DatabaseSchema ().Tables;

        public static string ConnectionKey (this DataConnection it) => it?.ConnectionString;

        public static string TableName<T> (this DataConnection it) => TableName (it, typeof(T));

        public static string TableName (this DataConnection it, Type entityType) => it.MappingSchema.GetEntityDescriptor (entityType).TableName;

    }

}