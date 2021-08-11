using System;
using System.Collections.Generic;
using System.Text;

namespace Model.ViewModel
{
    public class ResultViewModel
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 回傳訊息
        /// </summary>
        public string Message { get; set; }
    }
}
