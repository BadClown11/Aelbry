using Aelbry.BO;

namespace Aelbry.DAL
{
    public class TeamDAL : MSSqlDatabase
    {
        public static TeamDAL Instance
        {
            get { return new TeamDAL(); }
        }

        private TeamDAL() : base("DefaultConnection")
        {
            _ = Validate.Instance;
        }

        public List<Team> GetByDepartment(int departmentId)
        {
            var list = new List<Team>();
            const string sSp = "SP_TEAM_GET_BY_DEPARTMENT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_DEPARTMENT_ID", departmentId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(MapTeam(reader));
                    }
                }
            }

            return list;
        }

        public Team GetById(int teamId)
        {
            Team team = null;
            const string sSp = "SP_TEAM_GET_BY_ID";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_TEAM_ID", teamId));

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        team = MapTeam(reader);
                    }
                }
            }

            return team;
        }

        public int Create(Team team)
        {
            const string sSp = "SP_TEAM_INSERT";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_DEPARTMENT_ID", team.DepartmentId));
                cmd.Parameters.Add(CreateParameter("@P_NAME", team.Name));
                cmd.Parameters.Add(CreateParameter("@P_CREATED_BY", team.CreatedBy));

                var pNewId = CreateParameterOut("@P_NEW_TEAM_ID", DbType.Int32, 4);
                cmd.Parameters.Add(pNewId);

                var pResult = CreateParameterOut("@OUT_RESULT", DbType.String, 400);
                cmd.Parameters.Add(pResult);

                cmd.ExecuteNonQuery();

                string result = Validate.getDefaultIfDBNull(pResult.Value, TypeCode.String).ToString();
                if (result != C.OK)
                {
                    throw new DataBaseException(result);
                }

                return Validate.getDefaultIntIfDBNull(pNewId.Value);
            }
        }

        public void Update(Team team)
        {
            const string sSp = "SP_TEAM_UPDATE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_TEAM_ID", team.TeamId));
                cmd.Parameters.Add(CreateParameter("@P_NAME", team.Name));
                cmd.Parameters.Add(CreateParameter("@P_IS_ACTIVE", team.IsActive));
                cmd.Parameters.Add(CreateParameter("@P_MODIFIED_BY", (object)team.ModifiedBy ?? DBNull.Value));

                var pResult = CreateParameterOut("@OUT_RESULT", DbType.String, 400);
                cmd.Parameters.Add(pResult);

                cmd.ExecuteNonQuery();

                string result = Validate.getDefaultIfDBNull(pResult.Value, TypeCode.String).ToString();
                if (result != C.OK)
                {
                    throw new DataBaseException(result);
                }
            }
        }

        public void Delete(int teamId, int modifiedBy)
        {
            const string sSp = "SP_TEAM_DELETE";

            using (var cmd = CreateStoredProcCommand(sSp, conn, txn))
            {
                cmd.Parameters.Add(CreateParameter("@P_TEAM_ID", teamId));
                cmd.Parameters.Add(CreateParameter("@P_MODIFIED_BY", modifiedBy));

                var pResult = CreateParameterOut("@OUT_RESULT", DbType.String, 400);
                cmd.Parameters.Add(pResult);

                cmd.ExecuteNonQuery();

                string result = Validate.getDefaultIfDBNull(pResult.Value, TypeCode.String).ToString();
                if (result != C.OK)
                {
                    throw new DataBaseException(result);
                }
            }
        }

        private Team MapTeam(IDataReader reader)
        {
            return new Team
            {
                TeamId = Validate.getDefaultIntIfDBNull(reader["TEAM_ID"]),
                DepartmentId = Validate.getDefaultIntIfDBNull(reader["DEPARTMENT_ID"]),
                Name = Validate.getDefaultStringIfDBNull(reader["NAME"]),
                IsActive = Validate.getDefaultBoolIfDBNull(reader["IS_ACTIVE"]),
                CreatedBy = Validate.getDefaultIntIfDBNull(reader["CREATED_BY"]),
                CreatedDate = Validate.getDefaultDateIfDBNull(reader["CREATED_DATE"]),
                ModifiedBy = Validate.getDefaultNullableIntIfDBNull(reader["MODIFIED_BY"]),
                ModifiedDate = Validate.getDefaultNullableDateIfDBNull(reader["MODIFIED_DATE"]),
            };
        }
    }
}
