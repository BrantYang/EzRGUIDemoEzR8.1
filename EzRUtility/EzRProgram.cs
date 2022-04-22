using Sick.EasyRanger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzRUtility
{
    public class EzRProgram
    {
        public ProcessingEnvironment Env { get { return _env; } set { _env = value; } }
        public List<string> ListSubProgName { get { return _listSubProgName; } }

        ProcessingEnvironment _env;
        List<string> _listSubProgName = new List<string>();
        public EzRProgram(string EzRFile)
        {
            _env = new ProcessingEnvironment();
            _env.Load(EzRFile);
            _listSubProgName = GetSubProgram();        
        }

        public List<string> GetSubProgram()
        {
            List<string> ListProg = new List<string>();
            foreach (StepProgram stp in _env.Programs)
            {
                ListProg.Add(stp.Name);
            }
            return ListProg;
        }

        public List<string> GetStepList(string SubProgName)
        {
            List<string> ListStepName = new List<string>();
            StepProgram stp = (StepProgram)_env.GetStepProgram(SubProgName);

            foreach (Step step in stp.StepList)
            {
                ListStepName.Add(stp.Name);
            }
            return ListStepName;
        }

        public void SaveBackup(string FilePath)
        {

        }

        public void RecoverEzRFromBackup(string FilePath)
        {

        }

        public int RunSubProgram(string StepProgramName)
        {
            if (_listSubProgName.Exists(t => t == StepProgramName))
            {
                var program = _env.GetStepProgram(StepProgramName);
                program.RunFromBeginning();
                return (int)program.TimeConsumed;
            }
            else
                return -1;
        }
    }
}
