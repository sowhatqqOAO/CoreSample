using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Model.ViewModel
{
    public class VMNews
    { 
        [DisplayName("主鍵")]
        public Guid Pk_News { get; set; }
 
        [DisplayName("標題")]
        public string News_Title { get; set; }

        [DisplayName("狀態")]
        public string News_State { get; set; }
 
        [DisplayName("內容")]
        public string News_Content { get; set; }

        #region 基本資訊包含日期/IP/建立人
        [DisplayName("建檔日期")]
        public DateTime? News_CreateDate { get; set; }

        [DisplayName("建立者IP")]
        public string News_CreateIp { get; set; }

        [DisplayName("建立者")]
        public string News_CreateCode { get; set; }

        [DisplayName("異動日期")]
        public DateTime? News_EditDate { get; set; }

        [DisplayName("異動者IP")]
        public string News_EditIp { get; set; }
        
        [DisplayName("最後修改者")]
        public string News_EditCode { get; set; }
        #endregion
    }
}
