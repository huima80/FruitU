<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
    <xsl:output method="html" indent="yes"/>
    <xsl:template match="/">
      <html>
        <link href="../css/PrintPO.css" rel="stylesheet" />
        <link href="http://apps.bdimg.com/libs/bootstrap/3.3.0/css/bootstrap.min.css" rel="stylesheet" />
        <title>订单打印</title>
        <div class="container-fluid print-po">
            <h1 class="text-center">Fruit U</h1>
            <div class="row">
                <div class="col-lg-10">
                    <p class="text-left">订单号：<span><xsl:value-of select="OrderID"/></span></p>
                    <p class="text-left">下单时间：<span><xsl:value-of select="OrderDate"/></span></p>
                    <p class="text-left">收货人：<span><xsl:value-of select="DeliverName"/></span>(<span><xsl:value-of select="DeliverPhone"/></span>)</p>
                    <p class="text-left">地址：<span><xsl:value-of select="DeliverAddress"/></span></p>
                    <p class="text-left">订单备注：<span><xsl:value-of select="OrderMemo"/></span></p>
                    <p class="text-center">
                        <table class="table table-condensed">
                            <tr>
                                <td class="text-center">商品名称</td>
                                <td class="text-center">数量</td>
                                <td class="text-center">单价</td>
                            </tr>
                            <xsl:for-each select="OrderDetailList">
                    <tr>
                        <td class="text-center"><xsl:value-of select="OrderProductName"/></td>
                        <td class="text-center"><xsl:value-of select="PurchaseQty"/></td>
                        <td class="text-center"><xsl:value-of select="PurchasePrice"/>元</td>
                    </tr>
                      </xsl:for-each>
                            <tr>
                                <td class="text-center">小计：</td>
                                <td class="text-right" colspan="2"><xsl:value-of select="OrderDetailPrice"/>元</td>
                            </tr>
                        </table>
                    </p>
                    <p class="text-right">运费：￥<span><xsl:value-of select="Freight"/></span></p>
                  <xsl:if test="MemberPointsDiscount != 0">
                      <p class="text-right">积分优惠：-￥<span><xsl:value-of select="MemberPointsDiscount"/></span></p>
                </xsl:if>
                  <xsl:if test="WxCardDiscount != 0">
                    <p class="text-right">
                        微信优惠券优惠：-￥<span><xsl:value-of select="WxCardDiscount"/></span>
                      <xsl:if test="WxCard/Title != 0">
                        <br />
                        <span>【<xsl:value-of select="WxCard/Title"/>}】</span>
                </xsl:if>
                    </p>
                </xsl:if>
                 <p class="order-price">订单总金额：￥<span><xsl:value-of select="OrderPrice"/></span></p>
                    <p class="text-right">
                        支付方式：<span>
                          <xsl:choose>
                            <xsl:when test="PaymentTerm = 1">微信支付</xsl:when>
                            <xsl:when test="PaymentTerm = 2">货到付款</xsl:when>
                            <xsl:when test="PaymentTerm = 3">支付宝</xsl:when>
                          </xsl:choose>
                        </span>
                    </p>
                </div>
            </div>
            <hr />
            <div class="row text-center">
                <div class="col-lg-10">
                    <h4>Fresh Fruits &amp; Juice</h4>
                    <h5>关注FruitU官方微信，尽享鲜果饮品！</h5>
                    <img class="qr-code" src="../images/FruitUQRCode430.jpg" />
                </div>
            </div>
            <div class="row text-center hidden-print">
                <div class="col-lg-10">
                    <input id="btnPrint" type="button" value="打印" class="btn btn-danger" onclick="window.print();" />
                    <input id="btnClose" type="button" value="关闭" class="btn btn-info" onclick="window.close();" />
                </div>
            </div>
        </div>
</html>
    </xsl:template>
</xsl:stylesheet>
