using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileUtility
{
    /// <summary>
    /// Excel加载结果类
    /// </summary>
    public class ExcelLoadResult
    {
        /// <summary>
        /// 数据
        /// </summary>
        public List<string[]> Data { get; set; }
        /// <summary>
        /// 列名
        /// </summary>
        public string[] ColumnNames { get; set; }
        /// <summary>
        /// 行数
        /// </summary>
        public int TotalRows { get; set; }
        /// <summary>
        /// 列数
        /// </summary>
        public int TotalColumns { get; set; }
        /// <summary>
        /// 加载事件戳
        /// </summary>
        public TimeSpan LoadTime { get; set; }
        /// <summary>
        /// 异常信息
        /// </summary>
        public string Error { get; set; }
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success => string.IsNullOrEmpty(Error);
    }
}
