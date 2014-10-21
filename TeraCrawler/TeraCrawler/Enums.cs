using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeraCrawler
{
    /// <summary>
    /// There are no comments for TargetSites in the schema.
    /// </summary>
    public enum TargetSites : int
    {

        /// <summary>
        /// There are no comments for TargetSites.hangame in the schema.
        /// </summary>
        hangame,
        /// <summary>
        /// There are no comments for TargetSites.inven in the schema.
        /// </summary>
        inven,
        /// <summary>
        /// There are no comments for TargetSites.naver in the schema.
        /// </summary>
        naver,
        /// <summary>
        /// There are no comments for TargetSites.thisisgame in the schema.
        /// </summary>
        thisisgame,
        /// <summary>
        /// There are no comments for TargetSites.gamemeca in the schema.
        /// </summary>
        gamemeca
    }

    /// <summary>
    /// There are no comments for Games in the schema.
    /// </summary>
    public enum Games : int
    {

        /// <summary>
        /// There are no comments for Games.tera in the schema.
        /// </summary>
        tera
    }

    /// <summary>
    /// There are no comments for LogType in the schema.
    /// </summary>
    public enum LogType : int
    {

        /// <summary>
        /// There are no comments for LogType.Info in the schema.
        /// </summary>
        Info,
        /// <summary>
        /// There are no comments for LogType.Exception in the schema.
        /// </summary>
        Exception
    }

    /// <summary>
    /// There are no comments for AnalysisPhase in the schema.
    /// </summary>
    public enum AnalysisPhase : int
    {

        /// <summary>
        /// There are no comments for AnalysisPhase.Preprocess in the schema.
        /// </summary>
        Preprocess
    }
}
