#include "xuartps.h"
#include "xgpiops.h"  // 2025.05.28
#include "xparameters.h"
#include "xil_io.h"
#include "led_config2.h" // for project_5_SenHard2
#include "xiltimer.h"

#include <unistd.h>


//#define UART_DEVICE_ID XPAR_XUARTPS_0_DEVICE_ID
//#define BUFFER_SIZE 100

#define LEDS_DEVICE_ID	0xa0000000
#define LED_CHANNEL 1
#define LED_DIRECTMASK 0x00

#define WAIT_CNT_MAX_LONG   (200000000U)
#define WAIT_CNT_MAX         (50000000U)

#define UART_DEVICE_ID 0xff010000
#define UART_DEVICE_ID_sen 0xff000000  // 2025.05.27

#define GPIO_DEVICE_ID 0xff0a0000  // 2025.05.28

#define BUFFER_SIZE 1
#define BUFFER_SIZE_ENC 6

#define BUFFER_SIZE_3B 10   // sync(0xFF) + sync(0xFF) + sync(0xFF) + sync(0xFF) + sync(0xFF), serial No(2byte), enc data(3byte)
#define BUFFER_SIZE_3B_DBL 10+3 // sync(0xFF) + sync(0xFF) + sync(0xFF) + sync(0xFF) + sync(0xFF), serial No(1byte), enc data(3byte), enc data(3byte), デリミタ(1byte) //2025.07.18
#define BUFFER_SIZE_3B_read 4   // sync(0xFF) + 3byte 
#define BUFFER_SIZE_3B_new 12   // sync(0xFF) X 6byte, serial No(2byte), enc data(3byte), delimta(1byte) // 2025.06.26
#define DATANUM_Per_10mS  1786


#define BUFFER_SIZE_PARSER 4  // expand because of sync+command

typedef	char					CHAR8;								/* 符号付き1バイト(8bit)							*/
typedef	short					SHORT16;							/* 符号付き2バイト(16bit)							*/
typedef	int						INT32;								/* 符号付き4バイト(32bit)							*/
typedef	long long				LONG64;								/* 符号付き8バイト(64bit)							*/
typedef	unsigned char			UCHAR8;								/* 符号無し1バイト(8bit)							*/
typedef	unsigned short			WORD16;								/* 符号無し2バイト(16bit)							*/
typedef	unsigned int			UINT32;								/* 符号無し4バイト(32bit)							*/
typedef	unsigned long long		DWORD64;							/* 符号無し8バイト(64bit)							*/
typedef	float					FLOAT32;							/* 浮動小数点型4バイト(32bit)						*/
typedef	double					DOUBLE64;							/* 浮動小数点型8バイト(64bit)						*/
typedef	INT32					BOOL;								/* ブーリアン型(32bit)								*/
typedef	void					VOID;								/* void型											*/
typedef	INT32					HRESULT;							/* 関数処理結果型(32bit)							*/


BOOL				g_bfTimerInt		= FALSE;									// タイマ割込フラグ


VOID			TimerInterrupt_R50(VOID);
u32 XUartPs_Recv_ito(XUartPs *InstancePtr, u8 *BufferPtr, u32 NumBytes);
u32 XUartPs_ReceiveBuffer_ito(XUartPs *InstancePtr);
void SendBurstBuffer();


    XGpioPs Gpio; // 2025.05.28 // gpio instance
    XGpioPs_Config *config_gpio;

    u32 read_data_32b_com=0x0;  
    u32 read_data_32b_com_2nd=0x0;  // 2025.07.18
    
    u8 RecvBuffer[BUFFER_SIZE];
    u8 SendBuffer[BUFFER_SIZE];

    u8 SendBuffer_3b[BUFFER_SIZE_3B_DBL];   //2025.07.18
    u8 SendBuffer_test[2] = {0x01,0x02};

    u8 Burst_Buffer[(DATANUM_Per_10mS-1)*(5+3)+13];   // Burst_Buffer[14293] // 2025.07.18

    u8 SendBuffer_pmod[7];  // 2025.08.25

    u8 RecvBufferEnc[BUFFER_SIZE_ENC];
    u8 RecvBufferEncPre[BUFFER_SIZE_ENC];
    
    u8 RecvBuffer_1b[BUFFER_SIZE];
    

    XUartPs Uart_Ps;
    XUartPs_Config *Config;
    XUartPs Uart_Ps_sen;  // 2025.05.27
    XUartPs_Config *Config_sen;  // 2025.05.27

////////////////////////////////////////////////
// register table initial value read 2025.06.16
////////////////////////////////////////////////
    // for VIVADO
    u32    reg_ENC_COM1 = 0x0;
    u32      reg_STATUS = 0x0;
    u32     reg_CONFIG1 = 0x0;
    u32     reg_EEP_ADR = 0x0;
    u32   reg_EEP_WDATA = 0x0;
    u32   reg_EEP_RDATA = 0x0;
    u32      reg_CM_1st = 0x0;
    u32      reg_CM_2nd = 0x0;
    u32 reg_ROT_ANG_1st = 0x0;
    u32 reg_ROT_ANG_2nd = 0x0;
    u32 reg_ROT_NUM_1st = 0x0;
    u32 reg_ROT_NUM_2nd = 0x0;

    u32     Sreg_CONFIG1 = 0x0003;  // 初期値を、三角波出力モードに変更。 2025.08.05

/////////////////////////////////
// 受信バッファのクリア関数
/////////////////////////////////

void ClearUartRxBuffer() {
    u8 data;
    while (XUartPs_IsReceiveData(Uart_Ps_sen.Config.BaseAddress)) {
        data = XUartPs_ReadReg(Uart_Ps_sen.Config.BaseAddress, XUARTPS_FIFO_OFFSET);
    }
}

//2025.06.25

/******************************************************************************
* Copyright (C) 2012 - 2020 Xilinx, Inc.  All rights reserved.
* Copyright (C) 2022 - 2024 Advanced Micro Devices, Inc. All Rights Reserved.
* SPDX-License-Identifier: MIT
******************************************************************************/

/*****************************************************************************/
/**
* @file  xtmrctr_fast_intr_example.c
*
* This file contains a design example using the timer counter driver
* (XTmCtr) and hardware device using fast interrupt mode.This example assumes
* that the interrupt controller is also present as a part of the system
*
*
* <pre>
* MODIFICATION HISTORY:
*
* Ver   Who  Date	 Changes
* ----- ---- -------- -----------------------------------------------
* 1.00a bss  07/31/12 First release
* 4.2   ms   01/23/17 Added xil_printf statement in main function to
*                     ensure that "Successfully ran" and "Failed" strings
*                     are available in all examples. This is a fix for
*                     CR-965028.
* 4.5   mus  07/05/18 Updated example to call TmrCtrDisableIntr function
*                     with correct arguments. Presently device id is
*                     being passed instead of interrupt id. It fixes
*                     CR#1006251.
* 4.5   mus  07/05/18 Fixed checkpatch errors and warnings.
* 4.12  ml   12/07/23 Make TimerExpired as a static variable.
* 4.12  mus  03/25/24 Update RESET_VALUE to reduce extecution time to 1 seconds.
*</pre>
******************************************************************************/

/***************************** Include Files *********************************/

#include "xtmrctr.h"
#include "xil_exception.h"
#include "xil_printf.h"
#include "xparameters.h"

/************************** Constant Definitions *****************************/
#ifdef SDT
#include "xinterrupt_wrap.h"

#define XTMRCTR_BASEADDRESS     XPAR_XTMRCTR_0_BASEADDR
#else
#include "xintc.h"
/*
 * The following constants map to the XPAR parameters created in the
 * xparameters.h file. They are only defined here such that a user can easily
 * change all the needed parameters in one place.
 */
#define TMRCTR_DEVICE_ID	XPAR_TMRCTR_0_DEVICE_ID
#define INTC_DEVICE_ID		XPAR_INTC_0_DEVICE_ID
#define TMRCTR_INTERRUPT_ID	XPAR_INTC_0_TMRCTR_0_VEC_ID
#endif

/*
 * The following constant determines which timer counter of the device that is
 * used for this example, there are currently 2 timer counters in a device
 * and this example uses the first one, 0, the timer numbers are 0 based
 */
#define TIMER_CNTR_0	 0

/*
 * The following constant is used to set the reset value of the timer counter,
 * making this number larger reduces the amount of time this example consumes
 * because it is the value the timer counter is loaded with when it is started
 */
//#define RESET_VALUE	 0xFFFF0000
//#define RESET_VALUE	 0xF8000000 // 100MHz 1.342s
//#define RESET_VALUE	 0xF0000000 // 100MHz 2.684s
//#define RESET_VALUE	 0xFFFFEC77 // 100MHz 50us for confirming 50us timer interrutp routine // 2025.06.25
//#define RESET_VALUE	 0xFFFE795F // 100MHz 50us for confirming 1ms timer interrutp routine // 2025.06.26 //Tec6と同様に1mS割り込み //2025.07.1
#define RESET_VALUE	 0xFFFFEA1F // 100MHz 56us for confirming 56us timer interrutp routine // 2025.06.27

/**************************** Type Definitions *******************************/

/***************** Macros (Inline Functions) Definitions *********************/

/************************** Function Prototypes ******************************/
#ifndef SDT
int TmrCtrFastIntrExample(XIntc *IntcInstancePtr,
			  XTmrCtr *InstancePtr,
			  u16 DeviceId,
			  u16 IntrId,
			  u8 TmrCtrNumber);

static int TmrCtrSetupIntrSystem(XIntc *IntcInstancePtr,
				 XTmrCtr *InstancePtr,
				 u16 DeviceId,
				 u16 IntrId,
				 u8 TmrCtrNumber);

static void TmrCtrDisableIntr(XIntc *IntcInstancePtr, u16 IntrId);
#else
int TmrCtrFastIntrExample(XTmrCtr *InstancePtr,
			  UINTPTR BaseAddr,
			  u8 TmrCtrNumber);

static void TmrCtrDisableIntr(XTmrCtr *InstancePtr);
#endif

static void TmrCtr_FastHandler(void) __attribute__ ((fast_interrupt));
static void TimerCounterHandler(void *CallBackRef, u8 TmrCtrNumber);

/************************** Variable Definitions *****************************/
#ifndef SDT
XIntc InterruptController;  /* The instance of the Interrupt Controller */
#endif

XTmrCtr TimerCounterInst;   /* The instance of the Timer Counter */

/*
 * The following variables are shared between non-interrupt processing and
 * interrupt processing such that they must be global.
 */
static volatile int TimerExpired;

int LOOP_NUM = 600000; // modify 50us timer interrupt confirm 2025.06.04 period=30s

int burst_flg=0;
/*****************************************************************************/
/**
* This is the main function of the Tmrctr example using Fast Interrupt feature
* in MicroBlaze and Intc controller.
*
* @param	None.
*
* @return	- XST_SUCCESS to indicate success.
*		- XST_FAILURE to indicate a failure.
*
* @note		None.
*
******************************************************************************/
u8  RecvBufferParser[BUFFER_SIZE_PARSER]; // 2025.06.26
u8 firstReadGo = 0;  //2025.08.27
u8 firstEncHandlerData[3];

#if 1
int main() {
	
    init_platform();

    XTimer_SetInterval(1);											// 割込周期セット 1mS周期     // 復活20250423 

	XTimer_ClearTickInterrupt();									// 割込クリア

    u8 init_com_flg=0;    
    u8 reg_write_com_flg=0;    
    u8 reg_read_com_flg=0;            
    u8 reg_read_com_3b_flg=0;    // add 20250418
    u8 read_dat_artifical=0xBB;
    
    unsigned int uiWaitCount;

    u32 write_data = 0x0;
    u32 read_data=0x0;  // 2025.06.25

    u8 RecvBufferEnc1byte[BUFFER_SIZE];
    u8 RecvBufferParserEnc[6];
    int ReceivedCount_sen;

    int Status;
    int Status_sen; // 2025.05.27

    // UART1の初期化
    Config = XUartPs_LookupConfig(UART_DEVICE_ID);
    if (Config == NULL) {
        return XST_FAILURE;
    }

    Status = XUartPs_CfgInitialize(&Uart_Ps, Config, Config->BaseAddress);
    if (Status != XST_SUCCESS) {
        return XST_FAILURE;
    }

    // UART_senの初期化 
    Config_sen = XUartPs_LookupConfig(UART_DEVICE_ID_sen); // 2025.05.27
    if (Config_sen == NULL) {
        return XST_FAILURE;
    }


    Status_sen = XUartPs_CfgInitialize(&Uart_Ps_sen, Config_sen, Config_sen->BaseAddress); // 2025.05.27
    
    if (Status_sen != XST_SUCCESS) {
        return XST_FAILURE;
    }
    


    Status_sen = XUartPs_SetBaudRate(&Uart_Ps_sen, 2500000);

    Status = XUartPs_SetBaudRate(&Uart_Ps, 1843200/*921600*/);  // 2025.06.27 // teratermでデータが現れないので、スピードをdefaultの115200に戻した。//2025.07.01 // 2025.07.16
    
    
    // GPIOの設定を取得
    config_gpio = XGpioPs_LookupConfig(GPIO_DEVICE_ID);    
    if (config_gpio == NULL) {
    return XST_FAILURE;
    }

    // GPIOの初期化
    Status = XGpioPs_CfgInitialize(&Gpio, config_gpio, config_gpio->BaseAddr);
    if (Status != XST_SUCCESS) {
    return XST_FAILURE;
    }


    // MIOピンの設定
    XGpioPs_SetDirectionPin(&Gpio, 72, 1); // MIO72を出力に設定
    XGpioPs_SetOutputEnablePin(&Gpio, 72, 1); // MIO72の出力を有効にする

    // ピンの状態を設定
    XGpioPs_WritePin(&Gpio, 72, 0); // MIO72をHighに設定
        
        // 2025.06.25
	/*
	 * Run the Timer Counter Fast Interrupt example.
	 */
#ifndef SDT
	Status = TmrCtrFastIntrExample(&InterruptController,
				       &TimerCounterInst,
				       TMRCTR_DEVICE_ID,
				       TMRCTR_INTERRUPT_ID,
				       TIMER_CNTR_0);
#else
	Status = TmrCtrFastIntrExample(&TimerCounterInst,
				       XTMRCTR_BASEADDRESS,
				       TIMER_CNTR_0);
#endif

	if (Status != XST_SUCCESS) {
		xil_printf("Tmrctr fast interrupt Example Failed\r\n");
		return XST_FAILURE;
	}

	xil_printf("Successfully ran Tmrctr fast interrupt Example\r\n");

    
    
    print("////////////\n\r");
    print("//  start //\n\r");
    print("////////////\n\r");

    ////////////////////////////////////////////// 
    // vivado レジスタの初期値をリード 2025.06.16
    //////////////////////////////////////////////
       reg_ENC_COM1 = Xil_In32(XPAR_LED_CONFIG2_PMOD_0_BASEADDR+0x00);
         reg_STATUS = Xil_In32(XPAR_LED_CONFIG2_PMOD_0_BASEADDR+0x04);
        reg_CONFIG1 = Xil_In32(XPAR_LED_CONFIG2_PMOD_0_BASEADDR+0x08);
        reg_EEP_ADR = Xil_In32(XPAR_LED_CONFIG2_PMOD_0_BASEADDR+0x0C);
      reg_EEP_WDATA = Xil_In32(XPAR_LED_CONFIG2_PMOD_0_BASEADDR+0x10);
      reg_EEP_RDATA = Xil_In32(XPAR_LED_CONFIG2_PMOD_0_BASEADDR+0x14);
         reg_CM_1st = Xil_In32(XPAR_LED_CONFIG2_PMOD_0_BASEADDR+0x18);
         reg_CM_2nd = Xil_In32(XPAR_LED_CONFIG2_PMOD_0_BASEADDR+0x1C);
    reg_ROT_ANG_1st = Xil_In32(XPAR_LED_CONFIG2_PMOD_0_BASEADDR+0x20);
    reg_ROT_ANG_2nd = Xil_In32(XPAR_LED_CONFIG2_PMOD_0_BASEADDR+0x24);
    reg_ROT_NUM_1st = Xil_In32(XPAR_LED_CONFIG2_PMOD_0_BASEADDR+0x28);
    reg_ROT_NUM_2nd = Xil_In32(XPAR_LED_CONFIG2_PMOD_0_BASEADDR+0x2C);


    ///////////////////////////////////////////////////////////////
    // 動作開始を示す為に、KD240ボード上のLEDを2個、1秒程度、点灯
    ///////////////////////////////////////////////////////////////
    write_data = 0x03;
    LED_CONFIG2_mWriteReg(XPAR_LED_CONFIG2_PMOD_0_BASEADDR, LED_CONFIG2_S00_AXI_SLV_REG0_OFFSET, write_data);  // Change the name to that defined in project_5_SenHard1 //led turn on OK
    for (uiWaitCount = 0U; uiWaitCount < WAIT_CNT_MAX; uiWaitCount++);
    write_data = 0x0;
    LED_CONFIG2_mWriteReg(XPAR_LED_CONFIG2_PMOD_0_BASEADDR, LED_CONFIG2_S00_AXI_SLV_REG0_OFFSET, write_data);  // Change the name to that defined in project_5_SenHard1 //led turn on OK
    
    ////////////////////////////////
    // Parser operation start
    ////////////////////////////////

    while(1){
    //print("////////////\n\r");
      // データの受信
      int ReceivedCount = XUartPs_Recv(&Uart_Ps, RecvBuffer, BUFFER_SIZE);
      
      if (ReceivedCount > 0) {
          // 受信したデータを処理

         SendBuffer[0] = RecvBuffer[0];  

         RecvBufferParser[3] = RecvBufferParser[2];        
         RecvBufferParser[2] = RecvBufferParser[1];  
         RecvBufferParser[1] = RecvBufferParser[0];  
         RecvBufferParser[0] = RecvBuffer[0];  

         ///////////////////////////////////////
         // Zynq Initialize コマンドを検出する
         // 1秒程度、LEDを2個点灯
         ///////////////////////////////////////
         
         if ((RecvBufferParser[0] == 0x30) && (RecvBufferParser[1] == 0x30)) {
		
	    init_com_flg=1; // Zynq Initialize コマンドを認識
	    
            write_data = 0x03;
            LED_CONFIG2_mWriteReg(XPAR_LED_CONFIG2_PMOD_0_BASEADDR, LED_CONFIG2_S00_AXI_SLV_REG0_OFFSET, write_data);  // Change the name to that defined in project_5_SenHard1 //led turn on OK
            for (uiWaitCount = 0U; uiWaitCount < WAIT_CNT_MAX; uiWaitCount++);
            write_data = 0x0;
            LED_CONFIG2_mWriteReg(XPAR_LED_CONFIG2_PMOD_0_BASEADDR, LED_CONFIG2_S00_AXI_SLV_REG0_OFFSET, write_data);  // Change the name to that defined in project_5_SenHard1 //led turn on OK
            
	    init_com_flg=0;            
         }

         ////////////////////////////////////////////////
         // Zynq 1byte Register Write コマンドを検出する
         ////////////////////////////////////////////////
         if ((RecvBufferParser[3] == 0xFF) && (RecvBufferParser[2] == 0x11)) { 

		
	    reg_write_com_flg=1; // Register Write コマンドを認識
	    //                                                 address              data
            LED_CONFIG2_mWriteReg(XPAR_LED_CONFIG2_PMOD_0_BASEADDR, RecvBufferParser[1], RecvBufferParser[0]);

            ///////////////////////////////////
            // stored in registers 2025.06.16
            ///////////////////////////////////
            switch(RecvBufferParser[1]){
                 case 0x00:
                        reg_ENC_COM1 = Xil_In32(XPAR_LED_CONFIG2_PMOD_0_BASEADDR+0x00); // Vivado上レジスタ
                        break;
                 case 0x04:
                    reg_STATUS = Xil_In32(XPAR_LED_CONFIG2_PMOD_0_BASEADDR+0x04); // Vivado上レジスタ
                    break;
                 case 0x08:
                         reg_CONFIG1 = Xil_In32(XPAR_LED_CONFIG2_PMOD_0_BASEADDR+0x08);  // Vivado上レジスタ
                   Sreg_CONFIG1 = RecvBufferParser[0];                        // vitis上レジスタ
                     break;
                 case 0x0C:
                         reg_EEP_ADR = Xil_In32(XPAR_LED_CONFIG2_PMOD_0_BASEADDR+0x0C); // Vivado上レジスタ
                         break;
                 case 0x10:
                       reg_EEP_WDATA = Xil_In32(XPAR_LED_CONFIG2_PMOD_0_BASEADDR+0x10); // Vivado上レジスタ
                       break;
                 case 0x14:
                       reg_EEP_RDATA = Xil_In32(XPAR_LED_CONFIG2_PMOD_0_BASEADDR+0x14); // Vivado上レジスタ
                       break;
                 case 0x18:
                          reg_CM_1st = Xil_In32(XPAR_LED_CONFIG2_PMOD_0_BASEADDR+0x18); // Vivado上レジスタ
                          break;
                 case 0x1C:
                          reg_CM_2nd = Xil_In32(XPAR_LED_CONFIG2_PMOD_0_BASEADDR+0x1C); // Vivado上レジスタ
                          break;
                 case 0x20:
                     reg_ROT_ANG_1st = Xil_In32(XPAR_LED_CONFIG2_PMOD_0_BASEADDR+0x20); // Vivado上レジスタ
                     break;
                 case 0x24:
                     reg_ROT_ANG_2nd = Xil_In32(XPAR_LED_CONFIG2_PMOD_0_BASEADDR+0x24); // Vivado上レジスタ
                     break;
                 case 0x28:
                     reg_ROT_NUM_1st = Xil_In32(XPAR_LED_CONFIG2_PMOD_0_BASEADDR+0x28); // Vivado上レジスタ
                     break;
                 case 0x2C:
                     reg_ROT_NUM_2nd = Xil_In32(XPAR_LED_CONFIG2_PMOD_0_BASEADDR+0x2C); // Vivado上レジスタ
                     break;
             }

	    reg_write_com_flg=0;            
         }
         
         int i=0;

         ///////////////////////////////////////////////
         // Zynq 1byte Register Read コマンドを検出する
         ///////////////////////////////////////////////
         if ((RecvBufferParser[2] == 0xFF) && (RecvBufferParser[1] == 0x12)) {

	    reg_read_com_flg=1; // Register Read コマンドを認識
	    
            read_data = Xil_In32(XPAR_LED_CONFIG2_PMOD_0_BASEADDR+RecvBufferParser[0]);

            SendBuffer[0] = /*0x35*/read_data & 0xFF;  // transfer to byte data
         
            XUartPs_Send(&Uart_Ps, SendBuffer, BUFFER_SIZE); 
            
	    reg_read_com_flg=0;            
         }

         ///////////////////////////////////////////////
         // ROT_ANG_2ndリード コマンドを検出する 2025.07.25
         ///////////////////////////////////////////////
         if ((RecvBufferParser[2] == 0xFF) && (RecvBufferParser[1] == 0x14)) {
		
	    reg_read_com_flg=1; // Register Read コマンドを認識

	    //                                                 address              data  // 2025.07.31
            LED_CONFIG2_mWriteReg(XPAR_LED_CONFIG2_PMOD_0_BASEADDR, 0x00, 0x01);  // invoke sigle trigger
            LED_CONFIG2_mWriteReg(XPAR_LED_CONFIG2_PMOD_0_BASEADDR, 0x00, 0x00);
            usleep(30); // wait until data is output.
	    
            read_data = Xil_In32(XPAR_LED_CONFIG2_PMOD_0_BASEADDR+RecvBufferParser[0]);
            
            // syncを4byteに追加 2025.08.25
            SendBuffer_pmod[0] = 0xFF; // 1byte sync code // 2025.08.25
            SendBuffer_pmod[1] = 0xFF; // 1byte sync code // 2025.08.25
            SendBuffer_pmod[2] = 0xFF; // 1byte sync code // 2025.08.25
            SendBuffer_pmod[3] = 0xFF; // 1byte sync code // 2025.08.25
            SendBuffer_pmod[4] = 0x00;     
            SendBuffer_pmod[5] = (read_data >> 16) & 0xFF;
            SendBuffer_pmod[6] = (read_data >> 8) & 0xFF; 
            SendBuffer_pmod[7] = read_data & 0xFF;  // transfer read_data to SendBuffer_pmod // 2025.08.25

            XUartPs_Send(&Uart_Ps, SendBuffer_pmod, 8); // 4byte送信 //2025.08.25 // including sync code            
            
	    reg_read_com_flg=0;            
         }

         ///////////////////////////////////////////////
         // Zynq 3byte Register Read コマンドを検出する
         ///////////////////////////////////////////////
         if ((RecvBufferParser[2] == 0xFF) && (RecvBufferParser[1] == 0x13)) {
		
	    reg_read_com_3b_flg=1; // Register Read コマンドを認識

            firstReadGo = 1; // 2025.08.27
            while(firstReadGo != 0); // ハンドラでfirstReadGoが0になるのを待つ
            SendBuffer_3b[0] = 0xFF; // sync 0xFF
            SendBuffer_3b[1] = 0xFF; // sync 0xFF  
            SendBuffer_3b[2] = 0xFF; // sync 0xFF  
            SendBuffer_3b[3] = 0xFF; // sync 0xFF  
            SendBuffer_3b[4] = 0x00;     
            SendBuffer_3b[5] = firstEncHandlerData[0]/*RecvBufferEnc[4]*/;  // transfer byte data(read_data_32b_com[23:16]) to PC
            SendBuffer_3b[6] = firstEncHandlerData[1]/*RecvBufferEnc[3]*/;  // transfer byte data(read_data_32b_com[15:8]) to PC             
            SendBuffer_3b[7] = firstEncHandlerData[2]/*RecvBufferEnc[2]*/ ;  // transfer byte data(read_data_32b_com[7:0]) to PC 
            
            
            XUartPs_Send(&Uart_Ps, SendBuffer_3b, 8); 
            
	    reg_read_com_3b_flg=0;   
          read_data_32b_com++;         
         }

         
      }
      
      read_dat_artifical++; // while文中なので、カウントアップは頻回する。//

      if(burst_flg == 1){
          burst_flg = 0;
          SendBurstBuffer(); // 2025.07.10
      }

      
    }

    cleanup_platform();
    
// 2025.06.25
#ifndef SDT
	TmrCtrDisableIntr(IntcInstancePtr, IntrId);
#else
	//TmrCtrDisableIntr(TmrCtrInstancePtr);  // 2025.06.25 
	  TmrCtrDisableIntr(&TimerCounterInst);  // 2025.06.26	
#endif
    
    return 0;
}

// Uart送信バッファが64byteの為、分割送信モジュール // 2025.07.10
void SendBurstBuffer(){
    u32 SentCount = 0;
    u32 Remaining = 14293; // =1786*8+5=14293
    while (Remaining > 0) {
        // FIFOサイズに合わせて分割送信（例：最大64バイト）
        u32 ChunkSize = (Remaining > 64) ? 64 : Remaining;
        u32 Sent = XUartPs_Send(&Uart_Ps, &Burst_Buffer[SentCount], ChunkSize);
        SentCount += Sent;
        Remaining -= Sent;
        // 送信完了まで待機（必要に応じて）
        while (XUartPs_IsSending(&Uart_Ps)) {
           // wait
        }
    }
}


int i=0;int j=0;
int tim_count=0;
int num_count=0;

short ang_array[984] = { // up
                       1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,

                       0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,
                       0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,
                       0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,
                       0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,
                       0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,
                       0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,
                       0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,
                       0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,
                       0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,
                       0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,

                       0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,
                       0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,
                       0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,
                       0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,
                       0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,
                       0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,
                       0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,
                       0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,
                       0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,
                       0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,

                       0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,
                       0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,
                       0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,
                       0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,
                       0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,
                       0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,
                       0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,
                       0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,
                       
                       // down
                       15,14,13,12,11,10,9,8,7,6,5,4,3,2,1,0,

                       16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1,0,
                       16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1,0,
                       16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1,0,
                       16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1,0,
                       16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1,0,
                       16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1,0,
                       16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1,0,
                       16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1,0,
                       16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1,0,
                       16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1,0,

                       16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1,0,
                       16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1,0,
                       16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1,0,
                       16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1,0,
                       16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1,0,
                       16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1,0,
                       16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1,0,
                       16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1,0,
                       16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1,0,
                       16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1,0,

                       16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1,0,
                       16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1,0,
                       16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1,0,
                       16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1,0,
                       16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1,0,
                       16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1,0,
                       16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1,0,
                       16,15,14,13,12,11,10,9,8,7,6,5,4,3,2,1,0,};




VOID			TimerInterrupt_R50(VOID)
{
        if (tim_count++ == 49){  // in case of 1msTimer , 50ms interval
        tim_count=0;

            read_data_32b_com = 1000000*ang_array[(num_count++ % 984)];

            SendBuffer_3b[0] = 0xFF;  // sync 0xFF
            SendBuffer_3b[1] = 0xFF;  // sync 0xFF
            SendBuffer_3b[2] = 0xFF;  // sync 0xFF              
            SendBuffer_3b[3] = 0xFF;  // sync 0xFF              
            SendBuffer_3b[4] = 0xFF;  // sync 0xFF                 
            
            SendBuffer_3b[5] = (i >> 8) & 0xFF;  // transfer serial No  High byte to PC 
            SendBuffer_3b[6] = i & 0xFF;         // transfer byte data(read_data_32b_com[15:8]) to PC   assume up to 600, because enc data is send every 0.1s until 60s
            
            
            SendBuffer_3b[7] = (read_data_32b_com >> 16) & 0xFF;  // transfer byte data(read_data_32b_com[23:16]) to PC 
            SendBuffer_3b[8] = (read_data_32b_com >> 8) & 0xFF;  // transfer byte data(read_data_32b_com[15:8]) to PC 
            SendBuffer_3b[9] = read_data_32b_com & 0xFF;  // transfer byte data(read_data_32b_com[7:0]) to PC 

            if(Sreg_CONFIG1 && 0x0001)   // 2025.06.16
                   XUartPs_Send(&Uart_Ps, SendBuffer_3b, BUFFER_SIZE_3B); // Note: Transfer begins at array subscript 0.
            
            if (i++ == 600) 
               i = 0;
       }
	XTimer_ClearTickInterrupt();									// EOI
	return;
}


// for 実験
u32 XUartPs_Recv_ito(XUartPs *InstancePtr,
			  u8 *BufferPtr, u32 NumBytes)
{
	u32 ReceivedCount;
	u32 ImrRegister;

	/* Assert validates the input arguments */
	Xil_AssertNonvoid(InstancePtr != NULL);
	Xil_AssertNonvoid(BufferPtr != NULL);
	Xil_AssertNonvoid(InstancePtr->IsReady == XIL_COMPONENT_IS_READY);

#if defined  (XCLOCKING)
	Xil_ClockEnable(InstancePtr->Config.RefClk);
#endif
	/*
	 * Disable all the interrupts.
	 * This stops a previous operation that may be interrupt driven
	 */
	ImrRegister = XUartPs_ReadReg(InstancePtr->Config.BaseAddress,
				  XUARTPS_IMR_OFFSET);
	XUartPs_WriteReg(InstancePtr->Config.BaseAddress, XUARTPS_IDR_OFFSET,
		XUARTPS_IXR_MASK);

	/* Setup the buffer parameters */
	InstancePtr->ReceiveBuffer.RequestedBytes = NumBytes;
	InstancePtr->ReceiveBuffer.RemainingBytes = NumBytes;
	InstancePtr->ReceiveBuffer.NextBytePtr = BufferPtr;

	/* Receive the data from the device */
	//ReceivedCount = XUartPs_ReceiveBuffer(InstancePtr);
	ReceivedCount = XUartPs_ReceiveBuffer_ito(InstancePtr);

	/* Restore the interrupt state */
	XUartPs_WriteReg(InstancePtr->Config.BaseAddress, XUARTPS_IER_OFFSET,
		ImrRegister);

	return ReceivedCount;
}

// for 実験
u32 XUartPs_ReceiveBuffer_ito(XUartPs *InstancePtr)
{
	u32 CsrRegister;
	u32 ReceivedCount = 0U;
	u32 ByteStatusValue, EventData;
	u32 Event;

	/*
	 * Read the Channel Status Register to determine if there is any data in
	 * the RX FIFO
	 */
	CsrRegister = XUartPs_ReadReg(InstancePtr->Config.BaseAddress,
				XUARTPS_SR_OFFSET);

	/*
	 * Loop until there is no more data in RX FIFO or the specified
	 * number of bytes has been received
	 */
	while((ReceivedCount < InstancePtr->ReceiveBuffer.RemainingBytes)&&
		(((CsrRegister & XUARTPS_SR_RXEMPTY) == (u32)0))){

		if (InstancePtr->is_rxbs_error) {
			ByteStatusValue = XUartPs_ReadReg(
						InstancePtr->Config.BaseAddress,
						XUARTPS_RXBS_OFFSET);
			if((ByteStatusValue & XUARTPS_RXBS_MASK)!= (u32)0) {
				EventData = ByteStatusValue;
				Event = XUARTPS_EVENT_PARE_FRAME_BRKE;
				/*
				 * Call the application handler to indicate that there is a receive
				 * error or a break interrupt, if the application cares about the
				 * error it call a function to get the last errors.
				 */
				InstancePtr->Handler(InstancePtr->CallBackRef,
							Event, EventData);
			}
		}

		InstancePtr->ReceiveBuffer.NextBytePtr[ReceivedCount] =
			XUartPs_ReadReg(InstancePtr->Config.
				  BaseAddress,
				  XUARTPS_FIFO_OFFSET);

		ReceivedCount++;

		CsrRegister = XUartPs_ReadReg(InstancePtr->Config.BaseAddress,
								XUARTPS_SR_OFFSET);
	}
	InstancePtr->is_rxbs_error = 0;
	/*
	 * Update the receive buffer to reflect the number of bytes just
	 * received
	 */
	if(InstancePtr->ReceiveBuffer.NextBytePtr != NULL){
		InstancePtr->ReceiveBuffer.NextBytePtr += ReceivedCount;
	}
	InstancePtr->ReceiveBuffer.RemainingBytes -= ReceivedCount;

	return ReceivedCount;
}
#endif

/*****************************************************************************/
/**
* This function is the handler which performs processing for the timer counter.
* It is called from an interrupt context such that the amount of processing
* performed should be minimized.  It is called when the timer counter expires
* if interrupts are enabled.
*
* This handler provides an example of how to handle timer counter interrupts
* but is application specific.
*
* @param	CallBackRef is a pointer to the callback function
* @param	TmrCtrNumber is the number of the timer to which this
*		handler is associated with.
*
* @return	None.
*
* @note		None.
*
******************************************************************************/
u32 read_data;
int bst_count;



void TimerCounterHandler(void *CallBackRef, u8 TmrCtrNumber)
{
	XTmrCtr *InstancePtr = (XTmrCtr *)CallBackRef;
//        print("now in handler routine\n\r");  // append for 50us timer confirm 2025.06.04

	/*
	 * Check if the timer counter has expired, checking is not necessary
	 * since that's the reason this function is executed, this just shows
	 * how the callback reference can be used as a pointer to the instance
	 * of the timer counter that expired, increment a shared variable so
	 * the main thread of execution can see the timer expired
	 */
	if (XTmrCtr_IsExpired(InstancePtr, TmrCtrNumber)) {
                
#if 1 // 0626_1049

                if (tim_count/*++*/ == 0/*1*/){  // in case of 1msTimer , 50ms interval  //2025.07.01
                    bst_count=0;
			
                    XGpioPs_WritePin(&Gpio, 72, 1); // MIO72をHighに設定
                    SendBuffer[0] = 0x02;
                    XUartPs_Send(&Uart_Ps_sen, SendBuffer, BUFFER_SIZE); // 2025.05.27
                    usleep(5);
                    XGpioPs_WritePin(&Gpio, 72, 0); // MIO72をHighに設定       
                    XUartPs_Recv(&Uart_Ps_sen, RecvBufferEnc, BUFFER_SIZE_ENC); // 2度読みの理由を探る為ローカル関数に変更              
           
#if 1  // 2025.07.03
                    SendBuffer_3b[0] = 0xFF;  // sync 0xFF
                    SendBuffer_3b[1] = 0xFF;  // sync 0xFF
                    SendBuffer_3b[2] = 0xFF;  // sync 0xFF              
                    SendBuffer_3b[3] = 0xFF;  // sync 0xFF              
                    SendBuffer_3b[4] = 0xFF;  // sync 0xFF                 
                    SendBuffer_3b[5] = i & 0xFF;         // transfer byte data(read_data_32b_com[15:8]) to PC   assume up to 600, because enc data is send every 0.1s until 60s
                    
                    if (Sreg_CONFIG1 == 0x0002){
                       SendBuffer_3b[6] = RecvBufferEnc[4];  // transfer byte data(read_data_32b_com[23:16]) to PC 
                       SendBuffer_3b[7] = RecvBufferEnc[3];  // transfer byte data(read_data_32b_com[15:8]) to PC 
                       SendBuffer_3b[8] = RecvBufferEnc[2];  // transfer byte data(read_data_32b_com[7:0]) to PC 
#if 1
                       // 2nd encoder値をwaveformに表示する 2025.08.01
                       LED_CONFIG2_mWriteReg(XPAR_LED_CONFIG2_PMOD_0_BASEADDR, 0x00, 0x01);  // invoke sigle trigger
                       LED_CONFIG2_mWriteReg(XPAR_LED_CONFIG2_PMOD_0_BASEADDR, 0x00, 0x00);
                       usleep(30); // wait until data is output.
                       read_data = Xil_In32(XPAR_LED_CONFIG2_PMOD_0_BASEADDR + 0x24);                       

                       SendBuffer_3b[9]  = (read_data >> 16) & 0xFF; /*RecvBufferEnc[4];*/  // transfer byte data(read_data_32b_com[23:16]) to PC 
                       SendBuffer_3b[10] = (read_data >> 8) & 0xFF;  /*RecvBufferEnc[3];*/ // transfer byte data(read_data_32b_com[15:8]) to PC 
                       SendBuffer_3b[11] = read_data & 0xFF;         /*RecvBufferEnc[2];*/ // transfer byte data(read_data_32b_com[7:0]) to PC 
#endif

                    }
                    
                    if (Sreg_CONFIG1 == 0x0003){

                       read_data_32b_com = 1000000*ang_array[(num_count % 984)];
                       SendBuffer_3b[6] = (read_data_32b_com >> 16) & 0xFF;  // transfer byte data(read_data_32b_com[23:16]) to PC 
                       SendBuffer_3b[7] = (read_data_32b_com >> 8) & 0xFF;  // transfer byte data(read_data_32b_com[15:8]) to PC 
                       SendBuffer_3b[8] = read_data_32b_com & 0xFF;  // transfer byte data(read_data_32b_com[7:0]) to PC 

                       read_data_32b_com_2nd = 1000000*ang_array[((num_count++ + 8/*2*/) % 984 )  % 984];

                       SendBuffer_3b[9] = (read_data_32b_com_2nd >> 16) & 0xFF;  // transfer byte data(read_data_32b_com[23:16]) to PC // 2nd enc is half value
                       SendBuffer_3b[10] = (read_data_32b_com_2nd >> 8) & 0xFF;  // transfer byte data(read_data_32b_com[15:8]) to PC 
                       SendBuffer_3b[11] = read_data_32b_com_2nd & 0xFF;  // transfer byte data(read_data_32b_com[7:0]) to PC                        
                       
                    }

                    SendBuffer_3b[12] = 0x00;  // delimita                    
                    
#endif

                    for (bst_count=0;bst_count<13;bst_count++)  //2025.07.18
                          Burst_Buffer[bst_count] = SendBuffer_3b[bst_count];
                }
                else {  // 2025.07.03

                    XGpioPs_WritePin(&Gpio, 72, 1); // MIO72をHighに設定

                    SendBuffer[0] = 0x02;
                    XUartPs_Send(&Uart_Ps_sen, SendBuffer, BUFFER_SIZE); // 2025.05.27
                    usleep(5);
                    XGpioPs_WritePin(&Gpio, 72, 0); // MIO72をHighに設定       

                    XUartPs_Recv(&Uart_Ps_sen, RecvBufferEnc, BUFFER_SIZE_ENC); // 2度読みの理由を探る為ローカル関数に変更              

#if 1

                    SendBuffer_3b[0] = i & 0xFF;         // transfer byte data(read_data_32b_com[15:8]) to PC   assume up to 600, because enc data is send every 0.1s until 60s
                    
                    if  ((Sreg_CONFIG1 == 0x0001) && (firstReadGo == 1)) { // 2025.08.28
                       firstEncHandlerData[0] = RecvBufferEnc[4];  // transfer byte data(read_data_32b_com[23:16]) to PC 
                       firstEncHandlerData[1] = RecvBufferEnc[3];  // transfer byte data(read_data_32b_com[15:8]) to PC 
                       firstEncHandlerData[2] = RecvBufferEnc[2];  // transfer byte data(read_data_32b_com[7:0]) to PC 
                       firstReadGo = 0;  // firstReadGo reset
                    }
                    
                    if (Sreg_CONFIG1 == 0x0002){
                       SendBuffer_3b[1] = RecvBufferEnc[4];  // transfer byte data(read_data_32b_com[23:16]) to PC 
                       SendBuffer_3b[2] = RecvBufferEnc[3];  // transfer byte data(read_data_32b_com[15:8]) to PC 
                       SendBuffer_3b[3] = RecvBufferEnc[2];  // transfer byte data(read_data_32b_com[7:0]) to PC 

#if 1
                       // 2nd encoder値をwaveformに表示する 2025.08.01
                       LED_CONFIG2_mWriteReg(XPAR_LED_CONFIG2_PMOD_0_BASEADDR, 0x00, 0x01);  // invoke sigle trigger
                       LED_CONFIG2_mWriteReg(XPAR_LED_CONFIG2_PMOD_0_BASEADDR, 0x00, 0x00);
                       usleep(30); // wait until data is output.
                       //read_data = Xil_In32(XPAR_LED_CONFIG2_PMOD_0_BASEADDR+RecvBufferParser[0]);
                       read_data = Xil_In32(XPAR_LED_CONFIG2_PMOD_0_BASEADDR + 0x24);                       

                       SendBuffer_3b[4] = (read_data >> 16) & 0xFF; /*RecvBufferEnc[4];*/  // transfer byte data(read_data_32b_com[23:16]) to PC 
                       SendBuffer_3b[5] = (read_data >> 8) & 0xFF;  /*RecvBufferEnc[3];*/ // transfer byte data(read_data_32b_com[15:8]) to PC 
                       SendBuffer_3b[6] = read_data & 0xFF;         /*RecvBufferEnc[2];*/ // transfer byte data(read_data_32b_com[7:0]) to PC 
#endif
                    }
                    
                    if (Sreg_CONFIG1 == 0x0003){

                       read_data_32b_com = 1000000*ang_array[(num_count % 984)];

                       SendBuffer_3b[1] = (read_data_32b_com >> 16) & 0xFF;  // transfer byte data(read_data_32b_com[23:16]) to PC 
                       SendBuffer_3b[2] = (read_data_32b_com >> 8) & 0xFF;  // transfer byte data(read_data_32b_com[15:8]) to PC 
                       SendBuffer_3b[3] = read_data_32b_com & 0xFF;  // transfer byte data(read_data_32b_com[7:0]) to PC 

                       read_data_32b_com_2nd = 1000000*ang_array[((num_count++ + 8/*2*/) % 984)];

                       SendBuffer_3b[4] = (read_data_32b_com_2nd >> 16) & 0xFF;  // transfer byte data(read_data_32b_com[23:16]) to PC  // 2nd enc is half value
                       SendBuffer_3b[5] = (read_data_32b_com_2nd >> 8) & 0xFF;  // transfer byte data(read_data_32b_com[15:8]) to PC 
                       SendBuffer_3b[6] = read_data_32b_com_2nd & 0xFF;  // transfer byte data(read_data_32b_com[7:0]) to PC                        
                    }

                    SendBuffer_3b[7] = 0x00;  // delimita
                    
                    
#endif
                    for (j=0;j<8;j++)  //2025.07.18
                       Burst_Buffer[bst_count+(tim_count-1)*8+j] = SendBuffer_3b[j];                     

                    if((Sreg_CONFIG1 == 0x0002) || (Sreg_CONFIG1 == 0x0003))    // 2025.07.07
                         if (tim_count == DATANUM_Per_10mS-1 )                  // 2025.07.07
                              burst_flg = 1;

                }
                // tim_countのインクリメントと判定 2025.07.10
                tim_count++;
                if (tim_count == DATANUM_Per_10mS)  //2025.07.10
                    tim_count=0;                
#endif
                i++; // 2025.07.03
	}
}

#ifndef SDT
/*****************************************************************************/
/**
* This function setups the interrupt system such that interrupts can occur
* for the timer counter. This function is application specific since the actual
* system may or may not have an interrupt controller.  The timer counter could
* be directly connected to a processor without an interrupt controller.  The
* user should modify this function to fit the application.
*
* @param	IntcInstancePtr is a pointer to the Interrupt Controller
*		driver Instance.
* @param	TmrCtrInstancePtr is a pointer to the XTmrCtr driver Instance.
* @param	DeviceId is the XPAR_<TmrCtr_instance>_DEVICE_ID value from
*		xparameters.h.
* @param	IntrId is XPAR_<INTC_instance>_<TmrCtr_instance>_VEC_ID
*		value from xparameters.h.
* @param	TmrCtrNumber is the number of the timer to which this
*		handler is associated with.
*
* @return
*		- XST_SUCCESS if the initialization is successful
*		- XST_FAILURE if the initialization is not successful
*
* @note		None
*
*
******************************************************************************/
static int TmrCtrSetupIntrSystem(XIntc *IntcInstancePtr,
				 XTmrCtr *TmrCtrInstancePtr,
				 u16 DeviceId,
				 u16 IntrId,
				 u8 TmrCtrNumber)
{
	int Status;

	/*
	 * Initialize the interrupt controller driver so that
	 * it's ready to use, specify the device ID that is generated in
	 * xparameters.h
	 */
	Status = XIntc_Initialize(IntcInstancePtr, INTC_DEVICE_ID);
	if (Status != XST_SUCCESS) {
		return XST_FAILURE;
	}

	/*
	 * Connect a Fast handler that will be called when an interrupt
	 * for the device occurs.
	 */
	Status = XIntc_ConnectFastHandler(IntcInstancePtr, IntrId,
					  (XFastInterruptHandler)TmrCtr_FastHandler);
	if (Status != XST_SUCCESS) {
		return XST_FAILURE;
	}

	/*
	 * Start the interrupt controller such that interrupts are enabled for
	 * all devices that cause interrupts, specific real mode so that
	 * the timer counter can cause interrupts through the
	 * interrupt controller.
	 */
	Status = XIntc_Start(IntcInstancePtr, XIN_REAL_MODE);
	if (Status != XST_SUCCESS) {
		return XST_FAILURE;
	}

	/*
	 * Enable the interrupt for the timer counter
	 */
	XIntc_Enable(IntcInstancePtr, IntrId);

	/*
	 * Initialize the exception table.
	 */
	Xil_ExceptionInit();

	/*
	 * Register the interrupt controller handler with the exception table.
	 */
	Xil_ExceptionRegisterHandler(XIL_EXCEPTION_ID_INT,
				     (Xil_ExceptionHandler)
				     XIntc_InterruptHandler,
				     IntcInstancePtr);
	/*
	 * Enable non-critical exceptions.
	 */
	Xil_ExceptionEnable();

	return XST_SUCCESS;
}
#endif

/*****************************************************************************/
/**
*
* This function disables the interrupts for the Timer.
*
* @param	IntcInstancePtr is a reference to the Interrupt Controller
*		driver Instance.
* @param	IntrId is XPAR_<INTC_instance>_<Timer_instance>_VEC_ID
*		value from xparameters.h.
*
* @return	None.
*
* @note		None.
*
******************************************************************************/
#ifndef SDT
void TmrCtrDisableIntr(XIntc *IntcInstancePtr, u16 IntrId)
#else
void TmrCtrDisableIntr(XTmrCtr *TmrCtrInstancePtr)
#endif

{
#ifndef SDT
	/*
	 * Disable the interrupt for the timer counter
	 */
	XIntc_Disable(IntcInstancePtr, IntrId);
#else
	XDisableIntrId(TmrCtrInstancePtr->Config.IntrId, TmrCtrInstancePtr->Config.IntrParent);
#endif
}

/*****************************************************************************/
/**
*
* This is the Fast Interrupt Handler for the Timer.
*
* @return	None.
*
* @note		None.
*
****************************************************************************/
void TmrCtr_FastHandler(void)
{

	/* Call the TmrCtr Interrupt handler */
	XTmrCtr_InterruptHandler(&TimerCounterInst);
}


/*****************************************************************************/
/**
* This function does a minimal test on the timer counter device and driver as a
* design example.  The purpose of this function is to illustrate how to use the
* XTmrCtr component.  It initializes a timer counter and then sets it up in
* compare mode with auto reload such that a periodic interrupt is generated.
*
* This function uses interrupt driven mode of the timer counter.
*
* @param	IntcInstancePtr is a pointer to the Interrupt Controller
*		driver Instance
* @param	TmrCtrInstancePtr is a pointer to the XTmrCtr driver Instance
* @param	DeviceId is the XPAR_<TmrCtr_instance>_DEVICE_ID value from
*		xparameters.h
* @param	IntrId is XPAR_<INTC_instance>_<TmrCtr_instance>_VEC_ID
*		value from xparameters.h
* @param	TmrCtrNumber is the number of the timer to which this
*		handler is associated with.
*
* @return
*		- XST_SUCCESS if the Test is successful
*		- XST_FAILURE if the Test is not successful
*
* @note		This function contains an infinite loop such that if interrupts
*		are not working it may never return.
*
*****************************************************************************/
#ifndef SDT
int TmrCtrFastIntrExample(XIntc *IntcInstancePtr,
			  XTmrCtr *TmrCtrInstancePtr,
			  u16 DeviceId,
			  u16 IntrId,
			  u8 TmrCtrNumber)
#else
int TmrCtrFastIntrExample(XTmrCtr *TmrCtrInstancePtr,
			  UINTPTR BaseAddr,
			  u8 TmrCtrNumber)
#endif
{
	int Status;
	int LastTimerExpired = 0;

	/*
	 * Initialize the timer counter so that it's ready to use,
	 * specify the device ID that is generated in xparameters.h
	 */
#ifndef SDT
	Status = XTmrCtr_Initialize(TmrCtrInstancePtr, DeviceId);
#else
	Status = XTmrCtr_Initialize(TmrCtrInstancePtr, BaseAddr);
#endif
	if (Status != XST_SUCCESS) {
		return XST_FAILURE;
	}

	/*
	 * Perform a self-test to ensure that the hardware was built
	 * correctly, use the 1st timer in the device (0)
	 */
	Status = XTmrCtr_SelfTest(TmrCtrInstancePtr, TmrCtrNumber);
	if (Status != XST_SUCCESS) {
		return XST_FAILURE;
	}

	/*
	 * Connect the timer counter to the interrupt subsystem such that
	 * interrupts can occur.  This function is application specific.
	 */
#ifndef SDT
	Status = TmrCtrSetupIntrSystem(IntcInstancePtr,
				       TmrCtrInstancePtr,
				       DeviceId,
				       IntrId,
				       TmrCtrNumber);
#else
	Status = XSetupInterruptSystem(TmrCtrInstancePtr, TmrCtr_FastHandler, \
				       TmrCtrInstancePtr->Config.IntrId, TmrCtrInstancePtr->Config.IntrParent, \
				       XINTERRUPT_DEFAULT_PRIORITY);
#endif
	if (Status != XST_SUCCESS) {
		return XST_FAILURE;
	}

	/*
	 * Setup the handler for the timer counter that will be called from the
	 * interrupt context when the timer expires, specify a pointer to the
	 * timer counter driver instance as the callback reference so the
	 * handler is able to access the instance data
	 */
	XTmrCtr_SetHandler(TmrCtrInstancePtr, TimerCounterHandler,
			   TmrCtrInstancePtr);

	/*
	 * Enable the interrupt of the timer counter so interrupts will occur
	 * and use auto reload mode such that the timer counter will reload
	 * itself automatically and continue repeatedly, without this option
	 * it would expire once only
	 */
	XTmrCtr_SetOptions(TmrCtrInstancePtr, TmrCtrNumber,
			   XTC_INT_MODE_OPTION | XTC_AUTO_RELOAD_OPTION);

	/*
	 * Set a reset value for the timer counter such that it will expire
	 * eariler than letting it roll over from 0, the reset value is loaded
	 * into the timer counter when it is started
	 */
	XTmrCtr_SetResetValue(TmrCtrInstancePtr, TmrCtrNumber, RESET_VALUE);

	/*
	 * Start the timer counter such that it's incrementing by default,
	 * then wait for it to timeout a number of times
	 */
	XTmrCtr_Start(TmrCtrInstancePtr, TmrCtrNumber);
        #if 0  // 2025.06.25
 	while (1) {
		/*
		 * Wait for the first timer counter to expire as indicated by
		 * the shared variable which the handler will increment
		 */
		while (TimerExpired == LastTimerExpired) {
		}
		LastTimerExpired = TimerExpired;

		/*
		 * If it has expired a number of times, then stop the timer
		 * counter and stop this example
		 */
//		if (TimerExpired == 3) {
		if (TimerExpired == LOOP_NUM) {  // ito modified 2025.06.04

			XTmrCtr_Stop(TmrCtrInstancePtr, TmrCtrNumber);
			break;
		}
		
		if (TimerExpired % 20000 == 0)  // append for 50us timer confirm
                     print("indicate handler routine 20000 times passed\n\r");
	}
	#endif
#ifndef SDT
	TmrCtrDisableIntr(IntcInstancePtr, IntrId);
#else
	//TmrCtrDisableIntr(TmrCtrInstancePtr);  // これをコメントアウトする必要があった。↑のwhileを殺すと直ぐにTmrCtrDisableInterが効いてしまい、ハンドラでブレークが止まらなかった。2025.06.25
#endif
	return XST_SUCCESS;
}
