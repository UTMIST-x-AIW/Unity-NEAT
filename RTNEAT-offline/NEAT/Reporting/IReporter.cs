using System;

namespace RTNEAT_offline.NEAT.Reporting
{
    public interface IReporter
    {
        void Info(string message);
        void Warning(string message);
        void Error(string message);
        void Debug(string message);
    }
} 