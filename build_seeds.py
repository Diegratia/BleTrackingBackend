import os

sql_file = 'setup_people_tracking_db.sql'
out_file = 'Shared/Repositories/Seeding/SqlDumpData.cs'

if not os.path.exists(os.path.dirname(out_file)):
    os.makedirs(os.path.dirname(out_file))

with open(sql_file, 'r', encoding='utf-8') as f:
    sql_content = f.read()

# Escape double quotes for C# verbatim string
# In C# @"...", double quotes are escaped by doubling them: ""
csharp_sql = sql_content.replace('"', '""')

content = f'''using System;

namespace Repositories.Seeding
{{
    public static class SqlDumpData
    {{
        public const string SqlQuery = @"
{csharp_sql}
";
    }}
}}
'''

with open(out_file, 'w', encoding='utf-8') as f:
    f.write(content)

print(f"Successfully generated {out_file}")
